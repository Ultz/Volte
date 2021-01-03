package volte.commands.cmds.utilities

import com.jagrosh.jdautilities.command.Command
import com.jagrosh.jdautilities.command.CommandEvent
import net.dv8tion.jda.api.entities.Message
import volte.meta.Emoji
import volte.meta.categories.utility
import volte.meta.createEmbedBuilder
import kotlin.system.measureTimeMillis

class PingCommand : Command() {

    init {
        this.name = "ping"
        this.help = "Simple command to check API latency and gateway ping."
        this.guildOnly = true
        this.category = utility()
    }

    override fun execute(event: CommandEvent) {
        lateinit var message: Message
        val embed = event.createEmbedBuilder("${Emoji.OK_HAND} **Gateway**: ${event.jda.gatewayPing}ms\n")

        val apiLatency = measureTimeMillis {
            message = event.message.reply(embed.build()).complete()
        }

        message.editMessage(embed.appendDescription("${Emoji.CLAP} **REST**: ${apiLatency}ms").build())
            .reference(event.message).queue()
    }
}