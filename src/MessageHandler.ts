import { Message, Client, RichEmbed, User, MessageEmbedProvider } from "discord.js"
import { Utils } from "./Utils";
import { FakeDatabase } from "./FakeDatabase";

const parser = require("discord-command-parser")

const customRole = "Custom"

export class MessageHandler {

    constructor(private prefix: string) {}

    public handleMessage(message: Message, client: Client) {

        const parsedMessage = parser.parse(message, this.prefix)
        // console.log(parsedMessage)
        if (parsedMessage.code === 0) {
            switch(parsedMessage.command) {
                case("ping"): this.ping(message, parsedMessage.arguments, client); break
                case("sendEmbed"): this.sendEmbed(message, parsedMessage.arguments, client); break
                case("reaction"): this.reactionEmbed(message, parsedMessage.arguments, client); break
                case("giveCustom"): this.giveCustom(message, parsedMessage.arguments, client); break
                case("removeCustom"): this.removeCustom(message, parsedMessage.arguments, client); break
            }
        }
    }

    private async ping(message: Message, args: string[], client: Client) {
        message.channel.send("pong")
    }

    private async sendEmbed(message: Message, args: string[], client: Client) {
        const embed = new RichEmbed()

        if (args.length == 2) {
            embed.setColor(Utils.colors.primary)
                .addField(args[0], args[1])
                .setTimestamp()
            message.channel.send(embed)
        } else {
            const sentMessage = await message.channel.send("Command falsch verwendet!") as Message
            sentMessage.delete(2000)
        }
        message.delete(2000)
    }

    private async reactionEmbed(message: Message, args: string[], client: Client) {
        const embed = new RichEmbed()
        const emoji = args.length > 2 ? Utils.getEmoji(args[2], message) : Utils.getEmoji("Tofu", message)


        if (args.length > 1) {
            embed.setColor(Utils.colors.primary)
                .addField(args[0], args[1])
                .addField("react to this message with", `${emoji}`)
                .setTimestamp()
                // .setFooter("ID: ", `${id}`)

            const sentMessage = await message.channel.send(embed) as Message
            sentMessage.react(emoji)
            FakeDatabase.lastEmoji = emoji
            FakeDatabase.lastReactEmbed = sentMessage

        } else {
            const sentMessage = await message.channel.send("Wrong Usage!, usage: $reactionEmbed 'Titel' 'Message' 'Emoji'") as Message
            sentMessage.delete(3000)
        }
        message.delete(2000)
    }

    private async giveCustom(message: Message, args: string[], client: Client) {
        const embedMessage = FakeDatabase.lastReactEmbed
        const userArray: User[] = []

        for(const reaction of embedMessage.reactions) {
            if(reaction[0] === FakeDatabase.lastEmoji.identifier) {
               for(const user of reaction[1].users) {
                   if (!user[1].bot) userArray.push(user[1])
               }
               break
            }
        }

        const userCount = args[0] ? (parseInt(args[0]) < userArray.length ? parseInt(args[0]) : userArray.length) : Math.ceil(userArray.length / 4)

        userArray.sort( function() { return 0.5 - Math.random() } )

        for (let i = 0; i < userCount; i++) {
            userArray[i].lastMessage.guild.member(userArray[i]).addRole(message.guild.roles.find(r => r.name === customRole))
        }

        const sentMessage = await message.channel.send(`${userCount} Leuten die Rolle ${customRole} gegeben!`) as Message
        sentMessage.delete(3000)

    }

    private async removeCustom(message: Message, args: string[], client: Client) {
        const role = message.guild.roles.find(r => r.name === customRole)

        role.members.forEach(member => {
            member.removeRole(role)
        })

        const sentMessage = await message.channel.send(`Alle aus der Rolle ${customRole} entfernt!`) as Message
        sentMessage.delete(3000)
    }

}