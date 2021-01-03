package volte.meta.categories

import com.jagrosh.jdautilities.command.Command.Category
import com.jagrosh.jdautilities.command.CommandEvent
import volte.util.DiscordUtil

fun owner(): Category = owner
fun operator(): Category = operator
fun utility(): Category = utility

private val owner = Category("Owner", DiscordUtil::isBotOwner)
private val operator = Category("Operator", DiscordUtil::isOperator)
private val utility = Category("Utility", ::notBot)

private fun notBot(event: CommandEvent): Boolean {
    return !event.author.isBot
}

