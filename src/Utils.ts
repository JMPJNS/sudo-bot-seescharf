import { Message } from "discord.js";

export class Utils {
    static getEmoji(name: string, message: Message, defaultEmoji: string) {

        if (!name) {
           return message.guild.emojis.find(emoji => emoji.name === defaultEmoji)
        }

        if(name.startsWith("<:")) {
            return message.guild.emojis.find(emoji => emoji.id === name.split(":")[2].substring(0, name.split(":")[2].length - 1))
        } else if(name.startsWith(":")) {
            return message.guild.emojis.find(emoji => emoji.name === name.split(":")[1])
        } else {
            return message.guild.emojis.find(emoji => emoji.name === name) || message.guild.emojis.find(emoji => emoji.name === defaultEmoji)
        }
    }

    static colors = {
        primary: "#0099ff"
    }
}