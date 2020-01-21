import { Message } from "discord.js";

export class Utils {
    static getEmoji(name: string, message: Message) {
        const n = name.startsWith(":") ? name.split(":")[1] : name
        return message.guild.emojis.find(emoji => emoji.name === n)
    }

    static colors = {
        primary: "#0099ff"
    }
}