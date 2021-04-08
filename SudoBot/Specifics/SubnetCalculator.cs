using System;
using System.Linq;
using System.Net;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace SudoBot.Specifics
{
    public class SubnetCalculator
    {
        public static async void DoTheThing(string input, int subnetCount, CommandContext ctx)
        {
            int prefixLength = int.Parse(
                input.Split("/")[1]);

            if (prefixLength > 32)
            {
                throw new Exception("Prefix Length can't be above 32");
            }

            var ip = ParseIPInput(input);

            var actualPrefixLength = CalculateActualPrefix(prefixLength, subnetCount);
            var subnetMask = CalculateSubnetMask(actualPrefixLength);
            var baseNetworkAdress = CalculateAdressBase(ip, prefixLength);

            var descEmbed = new DiscordEmbedBuilder();

            descEmbed.AddField("IP", ConvertFromIntegerToIpAddress(ip));
            descEmbed.AddField("Subnet Mask", ConvertFromIntegerToIpAddress(subnetMask));
            descEmbed.AddField("Network Prefix Length", prefixLength.ToString());
            descEmbed.AddField("Subnet Count", subnetCount.ToString());
            descEmbed.AddField("Actual Network Prefix Length", actualPrefixLength.ToString());
            
            await ctx.RespondAsync(embed: descEmbed.Build());


            for(int i = 0; i<subnetCount; i++) {
                var subnetEmbed = new DiscordEmbedBuilder();
                subnetEmbed.WithTitle("Subnet " + (i+1));
                var currentBaseAdress = CalculateCurrentAdressBase(baseNetworkAdress, prefixLength, actualPrefixLength, i, subnetCount);
                var broadcastAdress = CalculateBroadcastAdress(currentBaseAdress, actualPrefixLength);
                var hostCount = CalcuateHostCount(actualPrefixLength);
                var hostRange = $"{ConvertFromIntegerToIpAddress(currentBaseAdress+1)} - {ConvertFromIntegerToIpAddress(broadcastAdress-1)}";

                subnetEmbed.AddField("Network Address", ConvertFromIntegerToIpAddress(currentBaseAdress));
                subnetEmbed.AddField("Broadcast Address", ConvertFromIntegerToIpAddress(broadcastAdress));
                subnetEmbed.AddField("Host Count", hostCount.ToString());
                subnetEmbed.AddField("Host Range", hostRange);

                await ctx.RespondAsync(embed: subnetEmbed.Build());
            }
        }

        static UInt32 CalculateCurrentAdressBase(UInt32 ipBase, int prefixLength, int actualPrefixLength, int count, int subnetCount) {
            var t = ipBase | (UInt32) ((UInt32) count << 32-prefixLength-binaryNumLength(subnetCount - 1));            
            return t;
        }

        static UInt32 RotateRight(UInt32 number, int bits) {
            return (number >> bits) | (number << (31 - bits));
        }

        static UInt32 CalculateAdressBase(UInt32 ip, int prefix) {
            UInt32 highestHost = (UInt32) Math.Pow(2, 32 - prefix) - 1;
            return ip & ~highestHost;
        }

        static UInt32 ParseIPInput(string input)
        {
            return input.Split("/")[0].Split(".").Aggregate(Convert.ToUInt32(0), (curr, next) =>
            {
                var i = UInt32.Parse(next);
                if (i > 255)
                {
                    throw new Exception("IP Segment can't be above 255");
                }
                
                return curr << 8 | i;
            });
        }
        
        static int ParsePrefixInput(string input)
        {
            throw new NotImplementedException();
        }
        
        static int CalculateActualPrefix(int prefix, int subnetCount)
        {
            if (subnetCount == 1)
                return prefix;
            
            var addedLength = binaryNumLength(subnetCount);
            
            return prefix + addedLength;
        }

        static int binaryNumLength(int number) {
            return Convert.ToString(number, 2).Length;
        }

        static UInt32 CalculateSubnetMask(int prefix)
        {
            return Convert.ToUInt32( ~0u << (32 - prefix + 1));
        }
        
        static string CalculateSubnetMaskString(int prefix)
        {
            var t = CalculateSubnetMask(prefix);
            return ConvertFromIntegerToIpAddress(t);
        }

        static UInt32 CalculateBroadcastAdress(UInt32 ip, int prefix)
        {
            var subnetMask = CalculateSubnetMask(prefix);
            var t = Convert.ToUInt32((~subnetMask) | ip);
            return t;
        }
        
        static string CalculateBroadcastAdressString(UInt32 ip, int prefix)
        {
            return ConvertFromIntegerToIpAddress(CalculateBroadcastAdress(ip, prefix));
        }

        static UInt32 CalcuateHostCount(int prefix)
        {
            return (~CalculateSubnetMask(prefix) - 1);
        }

        static (uint firstIp, uint lastIp) CalculateHostRange(UInt32 ip, int prefix)
        {
            var subnetMask = CalculateSubnetMask(prefix);
            var firstIp = Convert.ToUInt32((subnetMask & ip) + 1);
            var lastIp = CalculateBroadcastAdress(ip, prefix) - 1u;
            return (firstIp, lastIp);
        }

        static string CalculateHostRangeString(UInt32 ip, int prefix)
        {
            (UInt32 first, UInt32 last) t = CalculateHostRange(ip, prefix);
            return $"{ConvertFromIntegerToIpAddress(t.first)}...{ConvertFromIntegerToIpAddress(t.last)}";
        }

        public static string ConvertFromIntegerToIpAddress(uint ipAddress)
        {
            byte[] bytes = BitConverter.GetBytes(ipAddress);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return new IPAddress(bytes).ToString();
        }
    }
}