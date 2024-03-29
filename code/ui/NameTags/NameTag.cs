﻿using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Garryware.UI;

public class NameTag : WorldPanel
{
    public IClient Client;

    private Label place;
    private Label result;
    private Label name;

    private const float w = 1000;
    private const float h = 1000;
    private static readonly string[] PlaceEmojis = { "🥇", "🥈", "🥉" };
    
    // @todo: figure out why these are a bit washed out, lighting maybe?
    public NameTag()
    {
        PanelBounds = new Rect(-(w / 2), -(h / 2), w, h);
        StyleSheet.Load("/ui/NameTags/NameTag.scss");
        
        var column = Add.Panel("content");
        {
            var awards = column.Add.Panel("awards");
            {
                place = awards.Add.Label(string.Empty, "place");
                result = awards.Add.Label(string.Empty, "result");
            }
            name = column.Add.Label(string.Empty, "name");
        }
    }

    public void Update()
    {
        var clientPlace = Client.GetInt(Tags.Place);
        place.Text = clientPlace > 0 && clientPlace < PlaceEmojis.Length ? PlaceEmojis[clientPlace - 1] : string.Empty;
        name.Text = "" + Client.Name;

        if (Client.Pawn is GarrywarePlayer clientPlayer)
        {
            result.Text = clientPlayer.HasLockedInResult ? (clientPlayer.HasWonRound ? "✔" : "❌") : string.Empty;
        }
        
        PanelBounds = new Rect(-(w / 2), -(h / 2), w, h);
        Position = Client.Pawn.AimRay.Position + Vector3.Up * 24;
        Rotation = Rotation.LookAt(Game.LocalPawn.AimRay.Position - Transform.Position);
    }
}