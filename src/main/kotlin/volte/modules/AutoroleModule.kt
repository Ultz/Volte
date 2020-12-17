package volte.modules

import net.dv8tion.jda.api.events.guild.member.GuildMemberJoinEvent
import net.dv8tion.jda.api.hooks.ListenerAdapter
import volte.Volte
import volte.database.VolteDatabase
import volte.meta.getData

class AutoroleModule: ListenerAdapter() {

    override fun onGuildMemberJoin(event: GuildMemberJoinEvent) {
        val roleId: String = event.guild.getData().getAutorole()

        if (roleId.isNotEmpty()) {
            val role = event.guild.getRoleById(roleId)
            event.guild.addRoleToMember(event.member, role ?: return).queue()
        }
    }
}