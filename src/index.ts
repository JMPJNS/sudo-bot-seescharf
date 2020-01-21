import * as Discord from "discord.js"
import { MessageHandler } from "./messageHandler"
const client = new Discord.Client()
const secrets = require("../secrets.json") as Secrets

const mh = new MessageHandler()


client.once('ready', () => {
	console.log('Ready!')
})

client.on('message', message => {
	console.log(message.content)
})

client.login(secrets.botToken)