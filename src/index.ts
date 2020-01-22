import * as Discord from "discord.js"
import { MessageHandler } from "./MessageHandler"
import { Secrets } from "./types/types"

const client = new Discord.Client()
const secrets = require("../secrets.json") as Secrets

const mh = new MessageHandler("$")


client.once('ready', () => {
	console.log('Ready!')
})

client.on('message', async message => {
	mh.handleMessage(message, client)
})

client.login(secrets.botToken)