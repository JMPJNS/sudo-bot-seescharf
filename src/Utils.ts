import { Message } from "discord.js";

export class Utils {
    static getEmoji(name: string, message: Message) {
        if(name.startsWith("<:")) {
            return message.guild.emojis.find(emoji => emoji.id === name.split(":")[2].substring(0, name.split(":")[2].length - 1))
        } else if(name.startsWith(":")) {
            return message.guild.emojis.find(emoji => emoji.name === name.split(":")[1])
        } else {
            return message.guild.emojis.find(emoji => emoji.name === name)
        }
    }

    static colors = {
        primary: "#0099ff"
    }
}