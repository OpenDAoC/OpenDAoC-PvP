/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */

/* <--- SendMessage Standardization --->
*  All messages now use translation IDs to both
*  centralize their location and standardize the method
*  of message calls used throughout this project. All messages affected
*  are in English. Other languages are not yet supported.
* 
*  To  find a message at its source location, either use
*  the message body contained in the comment above the return
*  (e.g., // Message: This is a message.) or the
*  translation ID (e.g., "AdminCommands.Account.Description").
* 
*  To perform message changes, take note of your server settings.
*  If the `serverproperty` table setting `use_dblanguage`
*  is set to `True`, you must make your changes from the
*  `languagesystem` DB table.
* 
*  If the `serverproperty` table setting
*  `update_existing_db_system_sentences_from_files` is set to `True`,
*  perform changes to messages using the .txt files locate in "GameServer >
*  language > EN".
*
*  OPTIONAL: After changing a message, paste the new string
*  into the comment above the affected method(s). This is
*  done for ease of reference. */

using System;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.Database;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&dungeonranges",
	new [] { "&dung" },
	"Dungeon Level Range Display",
   ePrivLevel.Player,
	"Displays all dungeons and their level ranges",
	"/dungeonrange")]

	public class DungeonRangesCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (IsSpammingCommand(client.Player, "dungeonranges"))
				return;
			
			client.Out.SendCustomTextWindow("Dungeons | Level - Range", DungeonHelper.GetDungeonLevelInfo());
		}
	}
}
