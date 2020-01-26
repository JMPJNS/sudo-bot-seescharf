import { Message } from "discord.js";

export class TimedFunctions {
    static giveMember(message: Message) {
        const memberRole = message.guild.roles.get("313024320165249034")
        const youtubeRole = message.guild.roles.get("671075502911520768")
        const twitchRole = message.guild.roles.get("671075257947258891")

        youtubeRole && youtubeRole.members.forEach(member => {
            member.roles.has(memberRole.id) || member.addRoles([memberRole]).catch(console.trace)
        })

        twitchRole && twitchRole.members.forEach(member => {
            member.roles.has(memberRole.id) || member.addRoles([memberRole]).catch(console.trace)
        })

        memberRole && memberRole.members.forEach(member => {
            if (member.roles.has(youtubeRole.id) || member.roles.has(twitchRole.id)) {}
            else {
                member.removeRoles([memberRole]).catch(console.trace)
            }
        })
    }
}