import { Message, Client, RichEmbed } from "discord.js"
import { Utils } from "./Utils";
import { FakeDatabase } from "./FakeDatabase";

const parser = require("discord-command-parser")

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
                .addField("Reagiert auf diese Nachricht mit", `${emoji}`)
                .setTimestamp()
                // .setFooter("ID: ", `${id}`)

            const sentMessage = await message.channel.send(embed) as Message
            sentMessage.react(emoji)

            FakeDatabase.lastReactEmbed = sentMessage

        } else {
            const sentMessage = await message.channel.send("Command falsch verwendet!, usage: $reactionEmbed 'Titel' 'Nachricht' 'Emoji'") as Message
            sentMessage.delete(3000)
        }
        message.delete(2000)
    }

    private async giveCustom(message: Message, args: string[], client: Client) {
        const embedMessage = FakeDatabase.lastReactEmbed

        console.log(embedMessage.reactions)
    }

}