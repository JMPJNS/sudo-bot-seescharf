import { Message, Client } from "discord.js";

interface Secrets {
    botToken: string
}


interface MessageCommand {
    handler: (message: Message, args: string[], client: Client) => void
    args: string[]
}