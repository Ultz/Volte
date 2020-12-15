package volte.commands.parsers

import com.jagrosh.jdautilities.command.CommandEvent
import net.dv8tion.jda.api.entities.Member
import org.apache.commons.lang3.StringUtils
import volte.commands.parsers.abstractions.VolteArgumentParser
import volte.meta.asPromise
import volte.util.DiscordUtil

class MemberParser : VolteArgumentParser<Member?>() {
    override fun parse(event: CommandEvent, value: String): Member? {
        var member: Member? = if (StringUtils.isNumeric(value))
            event.guild.getMemberById(value) //id check
        else null

        if (member == null) {
            val members = event.guild.findMembers {
                it.effectiveName.equals(value, true) //username/nickname check
            }.get()
                if (members.size == 1) {
                    member = members.first()
                }
            }

        if (member == null) {
            val parsed = DiscordUtil.parseUser(value) //<@id> user mention check
            if (parsed != null) {
                member = event.guild.getMemberById(value)
            }
        }

        return member
    }
}