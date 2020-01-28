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
            member: "313024320165249034",
            youtube: "671075502911520768",
            twitch: "671075257947258891"
        }
    }
}