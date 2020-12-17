package volte.commands.cmds.utilities

import com.jagrosh.jdautilities.command.Command
import com.jagrosh.jdautilities.command.CommandEvent
import net.dv8tion.jda.api.entities.Message
import volte.meta.*

class PingCommand : Command() {

    init {
        this.name = "ping"
        this.help = "Simple command to check API latency and gateway ping."
        this.guildOnly = true
    }

    override fun execute(event: CommandEvent) {
        lateinit var message: Message
        val embed = event.createEmbedBuilder("${Emoji.OK_HAND} **Gateway**: ${event.jda.gatewayPing}ms\n")

        val apiLatency = stopwatch {
            message = event.message.reply(embed.build()).complete()
        }

        message.editMessage(embed.appendDescription("${Emoji.CLAP} **REST**: ${apiLatency}ms").build()).queue()
    }
}