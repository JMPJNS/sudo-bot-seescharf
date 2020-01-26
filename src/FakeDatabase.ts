import { Message, Emoji } from "discord.js";

export class FakeDatabase {
    static lastReactEmbed: Message[] = []
    static lastEmoji: Emoji[] = []
    static handlerSetup: Boolean[] = []

    static giveMemberInterval: number[] = []
}