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

 using DOL.Database;

 namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&release", new string[] { "&rel" },
		ePrivLevel.Player,
		"When you are dead you can '/release'. This will bring you back to your bindpoint!",
		"/release")]
	public class ReleaseCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (client.Player.CurrentRegion.IsRvR && !client.Player.CurrentRegion.IsDungeon || ServerProperties.Properties.EVENT_THIDRANKI || ServerProperties.Properties.EVENT_TUTORIAL)
			{
				client.Player.Release(eReleaseType.RvR, false);
				return;
			}

            if (args.Length > 1 && args[1].ToLower() == "city")
            {
	            if (ServerProperties.Properties.EVENT_THIDRANKI || ServerProperties.Properties.EVENT_TUTORIAL)
		            return;
				client.Player.Release(eReleaseType.City, false);
					return;
			}

            if (args.Length > 1 && args[1].ToLower() == "house")
            {
	            if (ServerProperties.Properties.EVENT_THIDRANKI || ServerProperties.Properties.EVENT_TUTORIAL)
		            return;
                client.Player.Release(eReleaseType.House, false);
                return;
            }
            
            if (args.Length > 1 && args[1].ToLower() == "random")
            {
	           
	            client.Player.Release(eReleaseType.Random, false);
	            return;
            }
            
            if (args.Length > 1 && args[1].ToLower() == "bind")
            {
	           
	            client.Player.Release(eReleaseType.Normal, false);
	            return;
            }
            
			client.Player.Release(eReleaseType.City, false);
		}
	}
}