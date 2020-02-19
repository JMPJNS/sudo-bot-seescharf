import { Message, Emoji } from "discord.js";

export class FakeDatabase {
    static lastReactEmbed: Message[] = []
    static lastEmoji: Emoji[] = []
    static handlerSetup: Boolean[] = []

    static giveMemberInterval: number[] = []

    static importantIds = {
        users: {
            JMP: "272809112851578881"
        },
        roles: {
            custom: "679729020325199887"
        }
    }
}