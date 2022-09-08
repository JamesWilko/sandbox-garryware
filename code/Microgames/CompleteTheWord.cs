using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class CompleteTheWord : Microgame
{
    class Word
    {
        public string TargetWord;
        public string FullTargetWord;
        public string MissingLetter;
        public string DecoyLetters;

        public Word(string word, string missing, string decoys)
        {
            TargetWord = word.Replace(missing, "?");
            FullTargetWord = word;
            MissingLetter = missing;
            DecoyLetters = decoys;
        }
    }

    private Word targetWord;
    private static ShuffledDeck<Word> wordsList = new();

    public CompleteTheWord()
    {
        Rules = MicrogameRules.LoseOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn | MicrogameRules.DontClearInstructions;
        ActionsUsedInGame = PlayerAction.PrimaryAttack;
        AcceptableRooms = new[] { MicrogameRoom.Boxes, MicrogameRoom.Empty };
        GameLength = 5;
        
        wordsList.Add(new ("GARRY", "Y", "DZWFN"));
        wordsList.Add(new ("SUNNY", "Y", "DZFBN"));
        wordsList.Add(new ("MONEY", "M", "AUIRV"));
        wordsList.Add(new ("HONEY", "H", "AUIRV"));
        wordsList.Add(new ("PARTY", "R", "NLDXQ"));
        wordsList.Add(new ("SCORE", "E", "DPAWZ"));
        wordsList.Add(new ("SUPER", "S", "MBNJL"));
        wordsList.Add(new ("HAPPY", "H", "BDWKQ"));
        wordsList.Add(new ("PRIME", "P", "SVLTZ"));
        wordsList.Add(new ("CRIME", "C", "KVBNT"));
        wordsList.Add(new ("DONGS", "G", "PHFRQ"));
        wordsList.Add(new ("LOSER", "L", "FWKBZ"));
        wordsList.Add(new ("WINNER", "W", "LVZYX"));
        wordsList.Add(new ("DINNER", "D", "LVZYX"));
        wordsList.Add(new ("GUARD", "G", "HMBPC"));
        wordsList.Add(new ("MOVER", "R", "TLNZ"));
        wordsList.Add(new ("SHAKER", "R", "BPWZM"));
        wordsList.Add(new ("TARGET", "T", "DPLMW"));
        wordsList.Shuffle();
    }
    
    public override void Setup()
    {
        targetWord = wordsList.Next();
        ShowInstructions("#microgame.get-ready");
    }

    public override void Start()
    {
        ShowInstructions(string.Format("Finish the word: {0}", targetWord.TargetWord)); // @localization
        GiveWeapon<Pistol>(To.Everyone);
        
        // Add the correct target
        var correct = new BreakableProp
        {
            Transform = Room.AboveBoxSpawnsDeck.Next().Transform,
            Model = CommonEntities.Crate,
            PhysicsEnabled = false,
            Indestructible = true,
            ShowWorldText = true,
            WorldText = targetWord.MissingLetter
        };
        AutoCleanup(correct);
        correct.Damaged += OnCorrectTargetDamaged;
        
        // Add the decoy targets
        for (int i = 0; i < targetWord.DecoyLetters.Length; ++i)
        {
            var crate = new BreakableProp
            {
                Transform = Room.AboveBoxSpawnsDeck.Next().Transform,
                Model = CommonEntities.Crate,
                PhysicsEnabled = false,
                Indestructible = true,
                ShowWorldText = true,
                WorldText = targetWord.DecoyLetters[i].ToString()
            };
            AutoCleanup(crate);
            crate.Damaged += OnIncorrectTargetDamaged;
        }

    }

    private void OnCorrectTargetDamaged(BreakableProp crate, Entity attacker)
    {
        if (attacker is GarrywarePlayer player && !player.HasLockedInResult)
        {
            player.FlagAsRoundWinner();
            player.RemoveWeapons();
        }
    }
    
    private void OnIncorrectTargetDamaged(BreakableProp crate, Entity attacker)
    {
        if (attacker is GarrywarePlayer player && !player.HasLockedInResult)
        {
            player.FlagAsRoundLoser();
            player.RemoveWeapons();
        }
    }

    public override void Finish()
    {
        RemoveAllWeapons();
        ShowInstructions(string.Format("The word was {0}!", targetWord.FullTargetWord));
    }

    public override void Cleanup()
    {
    }
    
}
