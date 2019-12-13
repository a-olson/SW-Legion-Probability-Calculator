using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace LegionCalculator
{
    public partial class Form1 : Form
    {

        /*
         * Author Details:
         * Andrew Hart (Qark on FFG forums)
         * 31 Oct 2018
         */
        public Form1()
        {
            InitializeComponent();
        }

        #region Inputs and Constants
        //Dice
        int
            iRedAttack,
            iBlackAttack,
            iWhiteAttack;
        bool
            bRedDefence,
            bWhiteDefence;
        //Attack Keywords
        int
            iRam,
            iImpact,
            iPierce,
            iPrecise;
        //Surges
        bool
            bSurgeToCrit,
            bSurgeToHit,
            bSurgeToBlock;
        int
            iCritical,
            iAttackerSurgeTokens,
            iDefenderSurgeTokens;
        //Tokens
        bool
            bDeflect;
        int
            iDodgeTokens,
            iShieldTokens,
            iAimTokens;
        //Cover
        bool
            bHeavyCover,
            bLightCover,
            bArmour;
        int iArmourX;

        //Defence
        bool Impervious;
        int UncannyLuck;
        int iDangerSense;

        //System
        int iItterations;
        static int iMaxDamage = 25;

        int iArrayConstruction = iMaxDamage + 1;
        static string sTabs = "\t\t";
        int iRerolls;
        #endregion


        void GetInfo()//Gets all the input data
        {
            //Dice

            iRedAttack = (int)updnRedAttack.Value;
            iBlackAttack = (int)updnBlackAttack.Value;
            iWhiteAttack = (int)updnWhiteAttack.Value;

            bRedDefence = rbtnRedDefence.Checked;
            bWhiteDefence = rbtnWhiteDefence.Checked;
            //Attack Keywords
            
            iImpact = (int)updnImpact.Value;
            iPierce = (int)updnPierce.Value;
            iPrecise = (int)updnPrecise.Value; //Aim token allows for 2 rerolls automattically.
            iRam = (int)updnRam.Value;
            //Surges

            bSurgeToCrit = rbtnSurgeToCrit.Checked;
            bSurgeToHit = rbtnSurgeToHits.Checked;
            bSurgeToBlock = ckbxSurgeToBlock.Checked;
            iCritical = (int)updnCritical.Value;
            iAttackerSurgeTokens = (int)updnAttackerSurgeTokens.Value;
            iDefenderSurgeTokens = (int)updnDefenceSurgeTokens.Value;
            //Tokens

            iAimTokens = (int)UpDnAimToken.Value;
            iDodgeTokens = (int)UpDnDodgeToken.Value;
            iShieldTokens = (int)updnShield.Value;
            //Cover

            bHeavyCover = rbtnHeavyCover.Checked;
            bLightCover = rbtnLightCover.Checked;
            bArmour = ckbxArmour.Checked;
            iArmourX = (int)UpDnArmourX.Value;
            //Deflect

            bDeflect = ckbxDeflect.Checked;
            if(bDeflect && iDodgeTokens > 0) { bSurgeToBlock = true; }
            //Defence

            UncannyLuck = (int)updnLuck.Value;
            iDangerSense = (int)UpDnDangerSense.Value;
            Impervious = ckbxImpervious.Checked;
            //System

            iRerolls = (2 + iPrecise);
            iItterations = (int)updnItterations.Value;
        }
        void RollAndConvertAttack(ref Dice die, ref int critical_used, ref int surge_tokens_used)//Rolls attack die and converts surges where appropriate
        {
            die.Roll();
            if (die.ReadResult() == "Surge")
            {
                if (bSurgeToCrit)
                {
                    die.Set("Crit");
                }
                else if (critical_used < iCritical)
                {
                    die.Set("Crit");
                    critical_used++;
                }
                else if (bSurgeToHit)
                {
                    die.Set("Hit");
                }
                else if (surge_tokens_used < iAttackerSurgeTokens)
                {
                    die.Set("Hit");
                    surge_tokens_used++;
                }
                else
                {
                    die.Set("Blank");
                }
            }
        }
        void RollAndConvertDefence(ref Dice die, ref int surge_tokens_used)//Rolls defence die and converts surges where appropriate
        {
            die.Roll();
            if(die.ReadResult() == "Surge")
            {
                if (bSurgeToBlock)
                {
                    die.Set("Block");
                }
                else if (surge_tokens_used < iDefenderSurgeTokens)
                {
                    die.Set("Block");
                    surge_tokens_used++;
                }
                else
                {
                    die.Set("Blank");
                }
            }

        }
        double FindMedian (int[] damage)
        {
            List<int> DamageValues = new List<int>();
            
            for(int i = 0; i <= iMaxDamage; i++)
            {
                for (int j = 0; j < damage[i]; j++)
                {
                    DamageValues.Add(i);
                }
                
            }//Populate List

            int[] DamageValuesArray = DamageValues.ToArray();
            Array.Sort(DamageValuesArray);
            if(DamageValuesArray.Length == 0)
            {
                return 0;
            }
            else if (DamageValuesArray.Length % 2 == 0)
            {
                //Count is even
                int a = DamageValuesArray[DamageValuesArray.Length / 2 - 1];
                int b = DamageValuesArray[DamageValuesArray.Length / 2];
                return (double)(a + b) / (double)2;
            }
            else
            {
                return DamageValuesArray[DamageValuesArray.Length / 2];
            }
        }
        double StandardDeviation(int[] damage)
        {
            List<int> DamageValues = new List<int>();

            for (int i = 0; i <= iMaxDamage; i++)
            {
                for (int j = 0; j < damage[i]; j++)
                {
                    DamageValues.Add(i);
                }

            }//Populate List

            int[] values = DamageValues.ToArray();

            double avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }
        void Output(int[] damage) //Creates the outout string and sets the output textbox's text.
        {
            string Output = "# Hits" + sTabs + "Probability (%)\n";
            int ModeValue = 0;
            double ModeProbability = 0;
            for (int i = 0; i <= iMaxDamage; i++)
            {
                double Probability = (double)damage[i] / (double)iItterations * (double)100;
                if(Probability > ModeProbability)
                {
                    ModeValue = i;
                    ModeProbability = Probability;
                }
                Output += i + sTabs + Probability + "\n";
            }

            Output += "\nAt Least" + sTabs + "Probability (%)\n";

            for (int i = iMaxDamage; i >= 0; i--)
            {
                double Probability = 0;
                for (int j = i; j <= iMaxDamage; j++)
                {
                    Probability += (double)damage[j] / (double)iItterations * (double)100;
                }
                Output += i + sTabs + Probability + "\n";
            }

            #region A M SD
            double Average = 0;
            double Median = 0;
            double StandardD = 0;

            //Find Average
            for (int i = 0; i <= iMaxDamage; i++)
            {
                Average += damage[i] * i;
            }
            Average /= (double)iItterations;

            Output += "\nAverage: " + Average + "\n";

            //Find Median
            Median = FindMedian(damage);
            Output += "Median: " + Median + "\n";

            //Mode
            Output += "Mode: " + ModeValue + "\n";

            //find SD
            StandardD = StandardDeviation(damage);
            Output += "Standard Deviation: " + StandardD + "\n";
            #endregion

            rtxtbxOutput.Text = Output;
        }

        #region Events
        private void btnSimulate_Click(object sender, EventArgs e)
        {
            GetInfo();
            int[] Damage = new int[iArrayConstruction];

            for (int i = 1; i <= iItterations; i++)
            {
                 probarCompletion.Value = (int)((double)i / (double)iItterations * (double)100);

                 int RolledHits = 0;
                 int RolledCrits = 0;
                 int RolledBlocks = 0;
                #region Attack Dice Generation Step 1
                List<Dice> ListOfAttackDice = new List<Dice>();

                 for (int j = 0; j < iRedAttack; j++)
                 {
                     Dice Die = new Dice("RedAttack");
                     ListOfAttackDice.Add(Die);
                 }

                 for (int j = 0; j < iBlackAttack; j++)
                 {
                     Dice Die = new Dice("BlackAttack");
                     ListOfAttackDice.Add(Die);
                 }

                 for (int j = 0; j < iWhiteAttack; j++)
                 {
                     Dice Die = new Dice("WhiteAttack");
                     ListOfAttackDice.Add(Die);
                 }
                #endregion
                #region Attack Step 4
                int CriticalUsed = 0;
                int SurgeTokensUsed = 0;
                int RamUsed = 0;
                //Roll attack pool
                foreach (Dice AttackDie in ListOfAttackDice)
                 {
                     Dice Die = AttackDie;
                     RollAndConvertAttack(ref Die, ref CriticalUsed, ref SurgeTokensUsed);
                 }
                //Spend Aim Tokens
                for(int aim_count = 0; aim_count < iAimTokens; aim_count++)
                 {
                    int RerolledDice = 0;
                    foreach (Dice AttackDie in ListOfAttackDice)
                     {
                         if (RerolledDice < iRerolls && AttackDie.ReadResult() == "Blank" && AttackDie.ReadColour() == "RedAttack")
                         {
                             Dice Die = AttackDie;
                             RollAndConvertAttack(ref Die, ref CriticalUsed, ref SurgeTokensUsed);
                             RerolledDice++;
                         }
                     }//Red
                    foreach (Dice AttackDie in ListOfAttackDice)
                     {
                         if (RerolledDice < iRerolls && AttackDie.ReadResult() == "Blank" && AttackDie.ReadColour() == "BlackAttack")
                         {
                             Dice Die = AttackDie;
                             RollAndConvertAttack(ref Die, ref CriticalUsed, ref SurgeTokensUsed);
                             RerolledDice++;
                         }
                     }//Black
                    foreach (Dice AttackDie in ListOfAttackDice)
                     {
                         if (RerolledDice < iRerolls && AttackDie.ReadResult() == "Blank" && AttackDie.ReadColour() == "WhiteAttack")
                         {
                             Dice Die = AttackDie;
                             RollAndConvertAttack(ref Die, ref CriticalUsed, ref SurgeTokensUsed);
                             RerolledDice++;
                         }
                     }//White
                }
                //Ram!
                foreach(Dice AttackDie in ListOfAttackDice)
                {
                    if(RamUsed < iRam)
                    {
                        if (AttackDie.ReadResult() == "Blank")
                        {
                            AttackDie.Set("Crit");
                            RamUsed++;
                        }
                    }
                }
                foreach (Dice AttackDie in ListOfAttackDice)
                {
                    if (RamUsed < iRam)
                    {
                        if (AttackDie.ReadResult() == "Hit")
                        {
                            AttackDie.Set("Crit");
                            RamUsed++;
                        }
                    }
                }
                //Tally Results
                foreach (Dice AttackDie in ListOfAttackDice)
                 {
                     if (AttackDie.ReadResult() == "Crit")
                     {
                         RolledCrits++;
                     }
                     else if (AttackDie.ReadResult() == "Hit")
                     {
                         RolledHits++;
                     }
                 }
                #endregion

                #region Modifications Step 5
                //Apply Cover
                if (bLightCover)
                 {
                     RolledHits -= 1;
                 }
                 else if (bHeavyCover)
                 {
                     RolledHits -= 2;
                 }
                //Apply Dodge
                RolledHits -= iDodgeTokens;
                //Safety Checks
                if (RolledHits < 0) { RolledHits = 0; }
                if (RolledCrits < 0) { RolledCrits = 0; } //Don't think this can actually occur

                //Convert for Armour and Impact
                if (bArmour)
                 {
                     if (RolledHits >= iImpact)
                     {
                         RolledCrits += iImpact;
                     }
                     else
                     {
                         RolledCrits += RolledHits;
                     }
                     RolledHits = 0;
                 }
                else //Armour X
                {
                    if (RolledHits >= iImpact)
                    {
                        RolledCrits += iImpact;
                        RolledHits -= iImpact;
                        RolledHits -= iArmourX;
                        if (RolledHits < 0) { RolledHits = 0; } //Safety Check
                    }
                    else
                    {
                        RolledCrits += RolledHits;
                        RolledHits = 0;
                    }
                }
                #endregion

                #region Defence Dice Generation Step 7
                int TotalDefenceDice = RolledCrits + RolledHits + iDangerSense;
                TotalDefenceDice -= iShieldTokens; //Roll less dice for the shields you use. The blocks are added later, in step 8.
                if(TotalDefenceDice < 0) { TotalDefenceDice = 0; }
                if (Impervious) { TotalDefenceDice += iPierce; }
                List<Dice> ListOfDefenceDice = new List<Dice>();

                 for (int j = 0; j < TotalDefenceDice; j++)
                 {
                     if (bRedDefence)
                     {
                         Dice Die = new Dice("RedDefence");
                         ListOfDefenceDice.Add(Die);
                     }
                     else if (bWhiteDefence)
                     {
                         Dice Die = new Dice("WhiteDefence");
                         ListOfDefenceDice.Add(Die);
                     }
                 }
                #endregion
                #region Defend Step 7
                int DefenderSurgeTokensUsed = 0;
                //Roll defence pool
                foreach (Dice DefenceDie in ListOfDefenceDice)
                 {
                     Dice Die = DefenceDie;
                     RollAndConvertDefence(ref Die, ref DefenderSurgeTokensUsed);
                 }
                //Luck
                if(UncannyLuck > 0)
                {
                    int RerolledDice = 0;
                    foreach (Dice DefenceDie in ListOfDefenceDice)
                    {
                        if(RerolledDice < UncannyLuck && DefenceDie.ReadResult() == "Blank")
                        {
                            Dice Die = DefenceDie;
                            RollAndConvertDefence(ref Die, ref DefenderSurgeTokensUsed);
                        }
                    }
                }
                foreach (Dice DefenceDie in ListOfDefenceDice)
                 {
                     if (DefenceDie.ReadResult() == "Block") { RolledBlocks++; }
                 }
                #endregion
                #region Modifications Step 8
                //Pierce
                RolledBlocks -= iPierce;
                if (RolledBlocks < 0) { RolledBlocks = 0; }
                //Shields
                RolledBlocks += iShieldTokens;
                 #endregion

                #region Compare Results Step 9
                 try
                 {
                    int DamageAmount = RolledHits + RolledCrits - RolledBlocks;
                    if(DamageAmount < 0) { DamageAmount = 0; }
                    Damage[DamageAmount]++;
                 }
                catch
                 {
                     MessageBox.Show("Error: Too much damage. Please decrease number of attack dice.", "Error!");
                     break;
                 }
                #endregion
            };

            Output(Damage);
        }


        private void rbtnSurgeToCrit_CheckedChanged(object sender, EventArgs e)
        {
            //not used
        }

        private void rbtnSurgeToHits_CheckedChanged(object sender, EventArgs e)
        {
            //not used
        }

        private void rbtnSurgesNone_CheckedChanged(object sender, EventArgs e)
        {
            //not used
        }

        private void updnAttackerSurgeTokens_ValueChanged(object sender, EventArgs e)
        {
            //not used
        }
        #endregion
    }

    class Dice
    {
        private string sResult;
        private string sColour;

        static private string
            sHit = "Hit",
            sCrit = "Crit",
            sSurge = "Surge",
            sBlock = "Block",
            sBlank = "Blank";

        private static string[]
            sRedAttack = { sHit, sHit, sHit, sHit, sHit, sCrit, sSurge, sBlank },
            sBlackAttack = { sHit, sHit, sHit, sCrit, sSurge, sBlank, sBlank, sBlank },
            sWhiteAttack = { sHit, sCrit, sSurge, sBlank, sBlank, sBlank, sBlank, sBlank },

            sRedDefence = { sBlock, sBlock, sBlock, sSurge, sBlank, sBlank },
            sWhiteDefence = { sBlock, sSurge, sBlank, sBlank, sBlank, sBlank };

        public Dice (string colour)
        {
            sColour = colour;
        }

        public string Roll()
        {
            int RollResult;
            if (sColour == "RedAttack" || sColour == "BlackAttack" || sColour == "WhiteAttack")
            {
                RollResult = StaticRandom.Rand(0, 8);
            }
            else if(sColour == "RedDefence" || sColour == "WhiteDefence")
            {
                RollResult = StaticRandom.Rand(0, 6);
            }
            else
            {
                sResult = sBlank;
                return sResult;
            }

            if(sColour == "RedAttack")
            {
                sResult = sRedAttack[RollResult];
            }
            else if(sColour == "BlackAttack")
            {
                sResult = sBlackAttack[RollResult];
            }
            else if (sColour == "WhiteAttack")
            {
                sResult = sWhiteAttack[RollResult];
            }
            else if (sColour == "RedDefence")
            {
                sResult = sRedDefence[RollResult];
            }
            else if (sColour == "WhiteDefence")
            {
                sResult = sWhiteDefence[RollResult];
            }

            return sResult;
        }

        public void Set(string result)
        {
            sResult = result;
        }

        #region read
        public string ReadResult()
        {
            return sResult;
        }

        public string ReadColour()
        {
            return sColour;
        }
#endregion
    }

    static class StaticRandom
    {
        static int seed = Environment.TickCount;

        static readonly ThreadLocal<Random> random =
            new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

        public static int Rand(int min, int max)
        {
            return random.Value.Next(min, max);
        }
    }
}
