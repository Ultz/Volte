package volte.commands.cmds.owner

import com.jagrosh.jdautilities.command.Command
import com.jagrosh.jdautilities.command.CommandEvent
import volte.meta.categories.owner
import volte.meta.replyInline
import volte.meta.then

class SetNameCommand : Command() {

    init {
        this.name = "setname"
        this.help = "Sets the name of the currently logged in bot."
        this.ownerCommand = true
        this.category = owner()
    }

    override fun execute(event: CommandEvent) {
        event.jda.selfUser.manager.setName(event.args) then {
            event.replyInline {
                setDescription("Set my username to **${event.args}**.")
            }
        }
    }
}