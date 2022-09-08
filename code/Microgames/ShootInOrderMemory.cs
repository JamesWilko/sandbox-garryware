namespace Garryware.Microgames;

public class ShootInOrderMemory : ShootInOrder
{

    public ShootInOrderMemory()
        : base()
    {
        CooldownLength = 4f;
    }
    
    protected override void BuildDifficultyDeck()
    {
        difficultyDeck.Clear();
        difficultyDeck.Add(3, 3);
        difficultyDeck.Add(4, 3);
        difficultyDeck.Add(5, 1);
        difficultyDeck.Shuffle();
    }

    protected override void ShowSetupInstruction()
    {
        ShowInstructions("#microgame.look-carefully");
    }
    
    public override void Start()
    {
        base.Start();

        // Hide the values
        foreach (var crate in propValues.Keys)
        {
            crate.ShowWorldText = false;
        }
    }
    
    protected override void ShowFinishInstruction()
    {
        ShowInstructions($"The correct order was {string.Join(", ", targetValues)}!"); // @localization
        
        // Show the values again after we finish
        foreach (var crate in propValues.Keys)
        {
            crate.ShowWorldText = true;
        }
    }
    
}
