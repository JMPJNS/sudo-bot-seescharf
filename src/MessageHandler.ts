import { Message, Client, RichEmbed, User, TextChannel } from "discord.js"
import { Utils } from "./Utils";
import { FakeDatabase } from "./FakeDatabase";
import { TimedFunctions } from "./TimedFunctions";

const parser = require("discord-command-parser")

const customRole = "Custom"
const defaultEmoji = "Stan_OWO_Tomato"

export class MessageHandler {

    constructor(private prefix: string) {}

    public handleMessage(message: Message, client: Client) {

        const parsedMessage = parser.parse(message, this.prefix)
        // console.log(parsedMessage)
        if (parsedMessage.code === 0) {
            switch(parsedMessage.command) {
                case("sendEmbed"): this.sendEmbed(message, parsedMessage.arguments, client); break
                case("reaction"): this.reactionEmbed(message, parsedMessage.arguments, client); break
                case("giveCustom"): this.giveCustom(message, parsedMessage.arguments, client); break
                case("removeCustom"): this.removeCustom(message, parsedMessage.arguments, client); break
                case("say"): this.say(message, parsedMessage.arguments, client); break
            }
        } else if (message.content.trim() === "<@!669237703530774554>") {
            message.channel.send(`o/ ${message.author}`)
        }
    }

    private checkPermissions(message: Message) {
        Utils.logMessage(message)
        if (message.guild.member(message.author).hasPermission(["MANAGE_MESSAGES", "MANAGE_ROLES"]) || message.author.id == FakeDatabase.importantIds.users.JMP) {
            return true
        } else {
            message.delete(3000)
            this.sendAndDeleteMessage(message, "Keine Rechte dazu!", 3000)
            return false
        }
    }

    private async sendAndDeleteMessage(message: Message, text: string, time: number) {
        const sentMessage = await message.channel.send(text) as Message
        Utils.logMessage(sentMessage)
        sentMessage.delete(time)
    }

    private async ping(message: Message, args: string[], client: Client) {
        Utils.logMessage(message)
        const sentMessage = await message.channel.send("pong") as Message
        Utils.logMessage(sentMessage)
        sentMessage.delete(5000)
        message.delete(3000)
    }

    private async setupAutoHandlers(message: Message, args: string[], client: Client) {
        message.delete(200)

        if (!FakeDatabase.handlerSetup[message.guild.id]) {
            this.sendAndDeleteMessage(message, "setting up Automatic Functions", 3000)
            FakeDatabase.handlerSetup[message.guild.id] = true

            TimedFunctions.giveMember(message)
            FakeDatabase.giveMemberInterval[message.guild.id] = setInterval(() => TimedFunctions.giveMember(message), 5 * 60 * 1000)
        }

    }

    private async sendEmbed(message: Message, args: string[], client: Client) {
        if (!this.checkPermissions(message)) return
        const embed = new RichEmbed()

        if (args.length == 2) {
            embed.setColor(Utils.colors.primary)
                .addField(args[0], args[1])
                .setTimestamp()
            message.channel.send(embed)
        } else {
            const sentMessage = await message.channel.send("Command falsch verwendet!") as Message
            console.log(`[${sentMessage.author.username}]`, sentMessage.content)
            sentMessage.delete(2000)
        }
        message.delete(2000)
    }

    private async say(message: Message, args: string[], client: Client) {
        if (!this.checkPermissions(message)) return

        if(args.length) {
            const channel = args[0].match(/(?<=\<\#)\d+(?=\>)/)
            if(channel){
                args.shift()
                const sendChannel = message.guild.channels.get(channel[0]) as TextChannel
                Utils.logMessage(await sendChannel.send(args.join(" ")) as Message)
            } else {
                Utils.logMessage(await message.channel.send(args.join(" ")) as Message)
            }
        }

        
        message.delete(200)
    }

    private async reactionEmbed(message: Message, args: string[], client: Client) {
        if (!this.checkPermissions(message)) return
        const embed = new RichEmbed()
        const emoji = message.guild.emojis.get(FakeDatabase.importantIds.emoji.ollekSlurp)


        if (args.length > 1) {
            embed.setColor(Utils.colors.primary)
                .addField(args[0], args[1])
                .addField("Reagiere auf diese Nachricht mit", `${emoji}`)
                .setTimestamp()
                // .setFooter("ID: ", `${id}`)

            const sentMessage = await message.channel.send(embed) as Message
            console.log(`[${sentMessage.author.username}]`, sentMessage.content)
            sentMessage.react(emoji).catch(e=> console.error)
            FakeDatabase.lastEmoji[message.guild.id] = emoji
            FakeDatabase.lastReactEmbed[message.guild.id] = sentMessage

        } else {
            const sentMessage = await message.channel.send("Falsch verwendet!, $reaction 'Titel' 'Message' 'Emoji'") as Message
            console.log(`[${sentMessage.author.username}]`, sentMessage.content)
            sentMessage.delete(3000)
        }
        message.delete(2000)
    }

    private async giveCustom(message: Message, args: string[], client: Client) {
        if (!this.checkPermissions(message)) return
        const embedMessage = FakeDatabase.lastReactEmbed[message.guild.id]

        if(!embedMessage) {
            this.sendAndDeleteMessage(message, "Erstelle zuerst eine reaction: $reaction", 3000)
            message.delete(3000)
            return
        }

        const userArray: User[] = []
        message.delete(3000)

        const role = message.guild.roles.get(FakeDatabase.importantIds.roles.custom)

        if (!role) {
            const sm = await message.channel.send(`Erstelle die Rolle ${customRole}!`) as Message
            sm.delete(5000)
            return
        }

        for(const reaction of embedMessage.reactions) {
            if(reaction[0] === FakeDatabase.lastEmoji[message.guild.id].identifier) {
               for(const user of reaction[1].users) {
                   if (!user[1].bot) userArray.push(user[1])
               }
               break
            }
        }

        const userCount = args[0] ? (parseInt(args[0]) < userArray.length ? parseInt(args[0]) : userArray.length) : Math.ceil(userArray.length / 4)

        userArray.sort( function() { return 0.5 - Math.random() } )

        let userListMessage = ""
        for (let i = 0; i < userCount; i++) {
            const user = message.guild.member(userArray[i])
            user.addRole(message.guild.roles.get(FakeDatabase.importantIds.roles.custom))

            userListMessage += i < (userCount - 1) ? `${user}, ` : `${user}`
        }

        const sentMessage = await message.channel.send(`${!(userCount > 1) ? (userCount === 0 ? "keiner hat " : userListMessage + " hat") : userListMessage + " haben"} die Rolle ${customRole} gekriegt!`) as Message
        Utils.logMessage(sentMessage)

    }

    private async removeCustom(message: Message, args: string[], client: Client) {
        if (!this.checkPermissions(message)) return
        const role = message.guild.roles.get(FakeDatabase.importantIds.roles.custom)
        message.delete(3000)

        role && role.members.forEach(member => {
            member.removeRole(role)
        })

        if (role) { this.sendAndDeleteMessage(message, `Alle aus der Rolle ${customRole} entfernt!`, 3000) }
        else {this.sendAndDeleteMessage(message, `Die Rolle ${customRole} exestiert nicht!`, 3000)}
    }

}