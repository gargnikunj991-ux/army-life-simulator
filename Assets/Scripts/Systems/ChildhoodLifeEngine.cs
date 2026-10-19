using System;
using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    public static class ChildhoodLifeEngine
    {
        /// <summary>
        /// Processes each 6-month semester of youth from Age 0.0 to 18.0.
        /// Handles physical development, schooling, sibling rivalries, and milestone dilemmas.
        /// </summary>
        public static void ProcessYouthSemester(SoldierProfile soldier, GameManager gm)
        {
            if (soldier == null) return;

            float age = soldier.age;

            // 1. Natural Growth & Health Baseline
            soldier.health = Mathf.Min(100f, soldier.health + 2f);

            // 2. Family Financial Upkeep (Child is supported by parents)
            if (soldier.lineage != null)
            {
                long familySupport = (soldier.lineage.startingNobleRank > NobleRank.Commoner) ? 100 : 25;
                soldier.cashBalance += familySupport;
            }

            // Ensure Clan & School are active and synced
            soldier.InitializeDefaultClanMembers();
            if (age >= 6.0f)
            {
                soldier.InitializeSchoolPeersIfEligible();

                // Dynamic academic performance reflecting user's tier difficulty:
                // City: difficult to cross 50
                // State: difficult to score 75
                // Imperial: 85-90 rare, 95+ legendary
                float targetPerf = SchoolHelper.CalculateTargetPerformance(soldier.schoolTier, soldier.intellect, soldier.foundationalTempering);
                if (soldier.schoolPerformance <= 0.1f)
                {
                    soldier.schoolPerformance = targetPerf;
                }
                else
                {
                    soldier.schoolPerformance = Mathf.Lerp(soldier.schoolPerformance, targetPerf, 0.20f);
                }

                if (age < 12.0f)
                {
                    soldier.primarySchoolScore = Mathf.Clamp((soldier.primarySchoolScore * 0.75f) + (soldier.schoolPerformance * 0.25f), 25f, 100f);
                }

                // Annual academic promotion check (Ages 7.0, 8.0, 9.0, 10.0, 11.0, 13.0, 14.0, 16.0, 17.0)
                if (Mathf.Approximately(age % 1.0f, 0f) && !Mathf.Approximately(soldier.lastPromotionCheckAge, age) && !Mathf.Approximately(age, 12.0f) && !Mathf.Approximately(age, 18.0f))
                {
                    ProcessAnnualSchoolPromotion(soldier, age);
                }
            }

            // 3. Age-Specific Milestones & Feed Logs for all 36 semesters!
            LogAgeMilestone(soldier, age);

            // 4. Milestone Interactive Dilemmas across Childhood
            TriggerYouthDilemmaIfEligible(soldier, age);

            // 5. Turning Point: Age 18.0 Graduation & 3-Stage Interactive National Military Ranking Exam!
            if (age >= 18.0f && soldier.currentRank == null)
            {
                ConductNationalMilitaryExam(soldier, gm);
            }
        }

        private static void LogAgeMilestone(SoldierProfile soldier, float age)
        {
            // Milestone logs for every single 6-month semester (0.5 to 17.5)
            if (Mathf.Approximately(age, 0.5f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("INFANCY",
                    $"<color=#00FFAA><b>[INFANCY • AGE 0.5]</b></color> You spoke your first words: <i>\"Ma-ma\"</i> and <i>\"Sword\"</i>! Your parents embrace you with joyful tears.");
            }
            else if (Mathf.Approximately(age, 1.0f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("GROWTH",
                    $"<b>[TODDLER • AGE 1.0]</b> You took your first independent steps across the courtyard tiles, balancing steadily without falling.");
            }
            else if (Mathf.Approximately(age, 1.5f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("GROWTH",
                    $"<b>[TODDLER • AGE 1.5]</b> You are babbling curious questions and playfully chasing butterflies in the family courtyard.");
            }
            else if (Mathf.Approximately(age, 2.0f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("GROWTH",
                    $"<b>[CHILDHOOD • AGE 2.0]</b> You survived a seasonal fever with high vitality. Your body's natural constitution is growing resilient (+2 Health).");
            }
            else if (Mathf.Approximately(age, 2.5f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                    $"<b>[EDUCATION • AGE 2.5]</b> Mother sits with you by candlelight, pointing out imperial provinces and ancestral hero names on an illuminated scroll (+2 Intellect).");
                soldier.intellect = Mathf.Min(100f, soldier.intellect + 2f);
            }
            else if (Mathf.Approximately(age, 3.0f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("GROWTH",
                    $"<b>[CHILDHOOD • AGE 3.0]</b> Your curiosity for martial arts begins to shine. You mimic the imperial soldiers marching beyond the district gates.");
            }
            else if (Mathf.Approximately(age, 3.5f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("WORLD NEWS",
                    $"<b>[IMPERIAL DISPATCH • AGE 3.5]</b> Town criers announce border skirmishes against Iron Federation mech patrols along the northern energy ridges.");
            }
            else if (Mathf.Approximately(age, 4.0f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                    $"<b>[FAMILY • AGE 4.0]</b> Your competitive sibling <b>{soldier.siblingRivalName}</b> challenged you over a wooden warship toy. Childhood sibling rivalry begins!");
            }
            else if (Mathf.Approximately(age, 4.5f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("CULTURE",
                    $"<b>[ANCESTRAL FESTIVAL • AGE 4.5]</b> At the annual Autumn Energy Lantern Feast, elder veterans recounted epic charges against Ouroboros beast cavalry (+2 Intellect).");
                soldier.intellect = Mathf.Min(100f, soldier.intellect + 2f);
            }
            else if (Mathf.Approximately(age, 5.0f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("GROWTH",
                    $"<b>[GROWTH • AGE 5.0]</b> You have grown tall and agile. You can sprint across the training grounds without catching your breath (+3 Fitness).");
                soldier.fitness = Mathf.Min(100f, soldier.fitness + 3f);
            }
            else if (Mathf.Approximately(age, 5.5f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("CENSUS",
                    $"<b>[IMPERIAL CENSUS • AGE 5.5]</b> An imperial clerk recorded your lineage and physical stature in the Valerian Registry of Youth.");
            }
            else if (Mathf.Approximately(age, 6.0f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("ACADEMY",
                    $"<color=#00D4FF><b>[PRIMARY SCHOOL • AGE 6.0]</b></color> You enrolled in district primary school. Daily literacy, mathematics, and basic physical drill begin!");
            }
            else if (Mathf.Approximately(age, 6.5f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("EDUCATION",
                    $"<b>[SCHOOL LESSONS • AGE 6.5]</b> You aced the continental geography exam, charting the borders between the Empire, the Iron Federation, and Ouroboros (+3 Intellect).");
                soldier.intellect = Mathf.Min(100f, soldier.intellect + 3f);
            }
            else if (Mathf.Approximately(age, 7.0f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("DISCIPLINE",
                    $"<b>[MARTIAL THEORY • AGE 7.0]</b> Your instructor introduces the history of the 12 Power Tiers and how ancient sects once defended the realm.");
            }
            else if (Mathf.Approximately(age, 7.5f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("SPORTS",
                    $"<b>[RELAY RACES • AGE 7.5]</b> You anchored your school's 400m obstacle sprint team to a podium finish! (+4 Fitness).");
                soldier.fitness = Mathf.Min(100f, soldier.fitness + 4f);
            }
            else if (Mathf.Approximately(age, 8.0f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("PARADE",
                    $"<color=#FFD700><b>[IMPERIAL VICTORY PARADE • AGE 8.0]</b></color> Standing on Father's shoulders, you watched Grand Duke mechanized regiments and glowing Tier-8 champions march triumphantly (+5 Morale)!");
            }
            else if (Mathf.Approximately(age, 8.5f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("TACTICS",
                    $"<b>[WAR ARCHIVES • AGE 8.5]</b> You read about the Great Continental War and the tactical balance of energy well lines (+3 Intellect).");
                soldier.intellect = Mathf.Min(100f, soldier.intellect + 3f);
            }
            else if (Mathf.Approximately(age, 9.0f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("CLAN LIFE",
                    $"<b>[CLAN AFFAIRS • AGE 9.0]</b> The family elders inspected your posture and spirit, remarking on your growing potential.");
            }
            else if (Mathf.Approximately(age, 9.5f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("SURVIVAL",
                    $"<b>[YOUTH CAMP • AGE 9.5]</b> You spent summer wilderness camp learning survival knots, trench ditch digging, and water purification (+3 Fitness, +2 Combat).");
                soldier.fitness = Mathf.Min(100f, soldier.fitness + 3f);
                soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 2f);
            }
            else if (Mathf.Approximately(age, 10.0f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("TEMPERING",
                    $"<color=#FFD700><b>[BODY TEMPERING • AGE 10.0]</b></color> You began youth foundational body tempering: distance running, pushups, and meridian breathing (+4 Fitness, +6 Foundational Tempering).");
                soldier.fitness = Mathf.Min(100f, soldier.fitness + 4f);
                soldier.foundationalTempering = Mathf.Min(100f, soldier.foundationalTempering + 6f);
            }
            else if (Mathf.Approximately(age, 10.5f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("NUTRITION",
                    $"<b>[BONE BROTH • AGE 10.5]</b> Your family prepared special bone-tempering herbs. Your joints feel stronger and more durable (+3 Health).");
                soldier.health = Mathf.Min(100f, soldier.health + 3f);
            }
            else if (Mathf.Approximately(age, 11.0f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("CLAN RIVALRY",
                    $"<b>[SIBLING TENSION • AGE 11.0]</b> Sibling rivalry with {soldier.siblingRivalName} intensifies as clan elders begin tracking your early exam projections.");
            }
            else if (Mathf.Approximately(age, 11.5f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("GEOPOLITICS",
                    $"<b>[ACADEMY BRIEFING • AGE 11.5]</b> Instructors lecture on the horrors of Ouroboros Dominion beast-mutated shock troops and how to exploit their blood frenzy (+3 Intellect).");
                soldier.intellect = Mathf.Min(100f, soldier.intellect + 3f);
            }
            else if (Mathf.Approximately(age, 12.0f))
            {
                ProcessPrimarySchoolGraduation(soldier);
                soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 5f);
            }
            else if (Mathf.Approximately(age, 12.5f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("DRILL",
                    $"<b>[CADET DRILLS • AGE 12.5]</b> You mastered close-order formation marching and bayonet charges under the barking command of Drill Sergeant Brant (+4 Fitness).");
                soldier.fitness = Mathf.Min(100f, soldier.fitness + 4f);
            }
            else if (Mathf.Approximately(age, 13.0f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("FIRING RANGE",
                    $"<b>[CARBINE QUALIFICATION • AGE 13.0]</b> You fired your first live energy training carbine, placing tight groupings on the 100m target silhouettes (+5 Combat Skill).");
                soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 5f);
            }
            else if (Mathf.Approximately(age, 13.5f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("ROMANCE",
                    $"<b>[CADET ACADEMY • AGE 13.5]</b> Fellow cadet {soldier.childhoodRomanceInterest} caught your eye across the firing lanes. A youthful spark begins.");
            }
            else if (Mathf.Approximately(age, 14.0f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("SPARRING",
                    $"<b>[SPARRING • AGE 14.0]</b> You competed in the cadet intra-school sparring tournament, mastering defensive stances and counter-strikes (+5 Combat Skill).");
                soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 5f);
            }
            else if (Mathf.Approximately(age, 14.5f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("MARTIAL STUDY",
                    $"<b>[ENERGY THEORY • AGE 14.5]</b> You studied the circulation of atmospheric qi through the 12 prime meridians (+4 Foundational Tempering).");
                soldier.foundationalTempering = Mathf.Min(100f, soldier.foundationalTempering + 4f);
            }
            else if (Mathf.Approximately(age, 15.0f))
            {
                // Transition from Middle School to Youth Academy (Ages 15.0 to 17.5)
                soldier.schoolLevel = SchoolLevel.YouthAcademy;
                if (soldier.schoolTier == SchoolTier.State1st && soldier.schoolPerformance >= 85f)
                {
                    soldier.schoolTier = SchoolTier.ImperialPinnacle;
                    soldier.isImperialScholarshipStudent = true;
                }
                soldier.InitializeSchoolPeersIfEligible();
                soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 5f);
                soldier.intellect = Mathf.Min(100f, soldier.intellect + 3f);

                string imperialTag = soldier.isImperialScholarshipStudent ? " <color=#FFD700>[IMPERIAL SCHOLARSHIP RECIPIENT]</color>" : "";
                GameEvents.OnFeedEventLogged?.Invoke("ACADEMY",
                    $"<color=#00FFAA><b>[MIDDLE SCHOOL GRADUATION & YOUTH ACADEMY • AGE 15.0]</b></color>\n" +
                    $"You graduated Middle School and matriculated into <color=#00D4FF><b>{soldier.schoolName}</b></color> ({SchoolHelper.GetTierTitle(soldier.schoolTier)})!{imperialTag}\n" +
                    $"The final 3 years of youth academy drills, advanced tactical simulations, and live carbine marksmanship begin! (+5 Combat Skill, +3 Intellect).");
            }
            else if (Mathf.Approximately(age, 15.5f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("MOUNTAIN RUCK",
                    $"<b>[HIGH-ALTITUDE RUCK • AGE 15.5]</b> You carried a 25kg weighted pack across 30km of rocky mountain pass in freezing rain! (+6 Fitness, +4 Health).");
                soldier.fitness = Mathf.Min(100f, soldier.fitness + 6f);
                soldier.health = Mathf.Min(100f, soldier.health + 4f);
            }
            else if (Mathf.Approximately(age, 16.0f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("EXAM PREP",
                    $"<b>[DISTRICT MOCK EXAM • AGE 16.0]</b> You took the preliminary district mock exam. Teachers commend your tactical reasoning (+5 Intellect, +5 Fitness).");
                soldier.intellect = Mathf.Min(100f, soldier.intellect + 5f);
                soldier.fitness = Mathf.Min(100f, soldier.fitness + 5f);
            }
            else if (Mathf.Approximately(age, 16.5f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("FINAL YEAR",
                    $"<b>[SENIOR CADET • AGE 16.5]</b> You reached your final senior year at the Cadet Academy. All eyes are on the upcoming National Military Ranking Exam.");
            }
            else if (Mathf.Approximately(age, 17.0f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("FINAL FOCUS",
                    $"<b>[WAR ENGINE PREP • AGE 17.0]</b> You enter the final intensive preparation semester. Every hour of sweat now decides your lifetime rank!");
            }
            else if (Mathf.Approximately(age, 17.5f))
            {
                soldier.mockExamScore = CalculateProjectedExamScore(soldier, out string projectedAcademy);
                soldier.tookMockExam = true;
                soldier.mockExamProjectedAcademy = projectedAcademy;

                GameEvents.OnFeedEventLogged?.Invoke("MOCK EXAM",
                    $"<color=#FF8800>==================================================\n" +
                    $"<b>[THE NATIONAL MOCK EXAM • AGE 17.5]</b>\n" +
                    $"You sat for the official preliminary trial ranking exam alongside candidates nationwide!\n" +
                    $"Mock Exam Score: <b>{soldier.mockExamScore} / 750</b>\n" +
                    $"Projected Placement: <color=#00FFAA><b>{projectedAcademy}</b></color>\n" +
                    $"Only 6 months remain before the final National Military Ranking Exam at Age 18.0!\n" +
                    $"==================================================</color>");
            }
        }

        private static void ProcessAnnualSchoolPromotion(SoldierProfile soldier, float age)
        {
            if (soldier == null || soldier.schoolLevel == SchoolLevel.None) return;
            soldier.lastPromotionCheckAge = age;

            float threshold = SchoolHelper.GetPromotionThreshold(soldier.schoolTier);
            if (soldier.schoolPerformance >= threshold)
            {
                if (soldier.schoolTier < SchoolTier.ImperialPinnacle)
                {
                    if (soldier.schoolTier == SchoolTier.State1st)
                    {
                        // Promotion from State 1st to Imperial Pinnacle!
                        soldier.schoolTier = SchoolTier.ImperialPinnacle;
                        soldier.isImperialScholarshipStudent = true;
                        soldier.timesPromotedSchool++;
                        soldier.meritRating = Mathf.Min(100f, soldier.meritRating + 15f);
                        soldier.clanSuccessionScore += 12;
                        soldier.parentRelationship = Mathf.Min(100f, soldier.parentRelationship + 15f);
                        soldier.InitializeSchoolPeersIfEligible();

                        GameEvents.OnFeedEventLogged?.Invoke("IMPERIAL SCOUTING",
                            $"<color=#FFD700>==================================================\n" +
                            $"<b>[IMPERIAL SCHOLARSHIP TRANSFER • AGE {age:F0}]</b>\n" +
                            $"Imperial Academy heralds arrived from Valeria Magna! With an extraordinary score of <b>{soldier.schoolPerformance:F1}%</b> in {SchoolHelper.GetTierTitle(SchoolTier.State1st)}, you were selected as the <b>Top Scholar of {soldier.birthState}</b>!\n" +
                            $"Transferred to: <color=#FFD700><b>THE GRAND IMPERIAL YOUTH ACADEMY (VALERIA MAGNA)</b></color>!\n" +
                            $"Your clan elders hosted a celebratory banquet across the province (+15 Merit, +12 Clan Standing).\n" +
                            $"==================================================</color>");
                    }
                    else
                    {
                        SchoolTier oldTier = soldier.schoolTier;
                        soldier.schoolTier++;
                        soldier.timesPromotedSchool++;
                        soldier.meritRating = Mathf.Min(100f, soldier.meritRating + 4f);
                        soldier.clanSuccessionScore += 3;
                        soldier.InitializeSchoolPeersIfEligible();

                        GameEvents.OnFeedEventLogged?.Invoke("PROMOTION",
                            $"<color=#00FFAA><b>[ANNUAL SCHOOL PROMOTION • AGE {age:F0}]</b></color> " +
                            $"With a top-tier performance of <b>{soldier.schoolPerformance:F1}%</b> (exceeding the {threshold:F0}% benchmark), you earned promotion from {SchoolHelper.GetTierTitle(oldTier)} to <color=#00D4FF><b>{soldier.schoolName}</b></color>! (+4 Merit, +3 Clan Standing).");
                    }
                }
                else
                {
                    GameEvents.OnFeedEventLogged?.Invoke("ACADEMIC HONOR",
                        $"<color=#FFD700><b>[IMPERIAL PINNACLE TOP SCHOLAR • AGE {age:F0}]</b></color> You defended your elite standing at the pinnacle of {soldier.schoolName} with an exceptional {soldier.schoolPerformance:F1}% score (+5 Merit)!");
                    soldier.meritRating = Mathf.Min(100f, soldier.meritRating + 5f);
                }
            }
            else if (soldier.schoolPerformance < 32f && soldier.schoolTier > SchoolTier.City3rd)
            {
                SchoolTier oldTier = soldier.schoolTier;
                soldier.schoolTier--;
                soldier.timesDemotedSchool++;
                soldier.meritRating = Mathf.Max(0f, soldier.meritRating - 4f);
                soldier.InitializeSchoolPeersIfEligible();

                GameEvents.OnFeedEventLogged?.Invoke("ACADEMIC REASSIGNMENT",
                    $"<color=#FF4444><b>[ANNUAL SCHOOL REASSIGNMENT • AGE {age:F0}]</b></color> " +
                    $"Failing to meet rigorous benchmarks ({soldier.schoolPerformance:F1}%), you were transferred down from {SchoolHelper.GetTierTitle(oldTier)} to <color=#FF8800><b>{soldier.schoolName}</b></color>. Sibling Lucian sneered at your report card (-4 Merit).");
            }
            else
            {
                GameEvents.OnFeedEventLogged?.Invoke("REPORT CARD",
                    $"<b>[ANNUAL REPORT CARD • AGE {age:F0}]</b> You concluded the academic year in <b>{soldier.schoolName}</b> with a score of <b>{soldier.schoolPerformance:F1}%</b>. You retained your seat in good standing.");
            }
        }

        private static void ProcessPrimarySchoolGraduation(SoldierProfile soldier)
        {
            if (soldier == null) return;

            string grade;
            string honorsTitle;
            int meritBonus;
            int clanBonus;

            if (soldier.primarySchoolScore >= 88f)
            {
                grade = "A+";
                honorsTitle = "Imperial Valedictorian (Top 1% Honors)";
                meritBonus = 15;
                clanBonus = 10;
                soldier.primarySchoolGraduationHonors = honorsTitle;
                soldier.meritRating = Mathf.Min(100f, soldier.meritRating + meritBonus);
                soldier.clanSuccessionScore += clanBonus;
                soldier.parentRelationship = Mathf.Min(100f, soldier.parentRelationship + 10f);

                if (soldier.schoolTier < SchoolTier.State2nd)
                {
                    soldier.schoolTier = SchoolTier.State2nd;
                }
            }
            else if (soldier.primarySchoolScore >= 70f)
            {
                grade = "A";
                honorsTitle = "Distinguished Honors Graduate";
                meritBonus = 8;
                clanBonus = 5;
                soldier.primarySchoolGraduationHonors = honorsTitle;
                soldier.meritRating = Mathf.Min(100f, soldier.meritRating + meritBonus);
                soldier.clanSuccessionScore += clanBonus;

                if (soldier.schoolTier < SchoolTier.City1st)
                {
                    soldier.schoolTier = SchoolTier.City1st;
                }
            }
            else if (soldier.primarySchoolScore >= 50f)
            {
                grade = "B/C";
                honorsTitle = "Standard Primary Graduate";
                meritBonus = 3;
                clanBonus = 1;
                soldier.primarySchoolGraduationHonors = honorsTitle;
                soldier.meritRating = Mathf.Min(100f, soldier.meritRating + meritBonus);
                soldier.clanSuccessionScore += clanBonus;
            }
            else
            {
                grade = "D";
                honorsTitle = "Remedial Pass";
                meritBonus = -5;
                clanBonus = -5;
                soldier.primarySchoolGraduationHonors = honorsTitle;
                soldier.meritRating = Mathf.Max(0f, soldier.meritRating + meritBonus);
                soldier.clanSuccessionScore = Math.Max(0, soldier.clanSuccessionScore + clanBonus);
            }

            // Transition from Primary School to Middle School (Ages 12 to 14)
            soldier.schoolLevel = SchoolLevel.MiddleSchool;
            soldier.InitializeSchoolPeersIfEligible();

            GameEvents.OnFeedEventLogged?.Invoke("GRADUATION",
                $"<color=#FFD700>==================================================\n" +
                $"<b>[PRIMARY SCHOOL COMMENCEMENT & GRADUATION • AGE 12.0]</b>\n" +
                $"Cumulative Academic Score: <b>{soldier.primarySchoolScore:F1} / 100</b> (Grade: <b>{grade}</b>)\n" +
                $"Graduation Standing: <b>{honorsTitle}</b>\n" +
                $"Merit & Clan Impact: <b>{(meritBonus >= 0 ? "+" : "")}{meritBonus} Merit</b> | <b>{(clanBonus >= 0 ? "+" : "")}{clanBonus} Clan Standing</b>\n" +
                $"Middle School Admission: Enrolled in <color=#00FFAA><b>{soldier.schoolName}</b></color> ({SchoolHelper.GetTierTitle(soldier.schoolTier)})!\n" +
                $"<i>Your primary school honors permanently count toward your Age 18.0 National Military Ranking Exam!</i>\n" +
                $"==================================================</color>");
        }

        private static void TriggerYouthDilemmaIfEligible(SoldierProfile soldier, float age)
        {
            // Rich multi-choice dilemmas at major developmental crossroads!
            if (Mathf.Approximately(age, 1.0f))
            {
                // Age 1.0: The Grasping Ritual (1st Birthday Tradition)
                GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                    "1ST BIRTHDAY TRADITION",
                    "The Grasping Ceremony (One-Year Milestone)",
                    "On your first birthday ritual, your family places traditional symbolic items before you. Which one do your tiny hands reach for?",
                    "A Wooden Toy Dagger (+8 Fitness, +5 Tempering)",
                    "An Ancient Military Scroll (+10 Intellect)",
                    "A Family Gold Sovereign (+Clan Favor, +$200 Vault)",
                    () => { soldier.fitness += 8f; soldier.foundationalTempering += 5f; GameEvents.OnSoldierStatsUpdated?.Invoke(soldier); },
                    () => { soldier.intellect += 10f; GameEvents.OnSoldierStatsUpdated?.Invoke(soldier); },
                    () => { soldier.clanFavor += 8f; soldier.cashBalance += 200; GameEvents.OnSoldierStatsUpdated?.Invoke(soldier); }
                );
            }
            else if (Mathf.Approximately(age, 2.0f))
            {
                // Age 2.0: Spilled Ancestral Vitality Tonic
                GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                    "COURTYARD DISCOVERY",
                    "The Spilled Vitality Tonic",
                    "While toddling near Father's meditation chamber, you spot a knocked-over crystal vial of glowing imperial bone-tempering tonic pooling on the stone table.",
                    "Drink the glowing liquid! (Vitality surge vs Meridian burn)",
                    "Call out loudly for Mother (+Family Relation)",
                    "Carefully wipe it clean with your sleeve (+Discipline)",
                    () =>
                    {
                        if (UnityEngine.Random.value < 0.7f)
                        {
                            soldier.foundationalTempering += 10f;
                            soldier.fitness += 5f;
                            GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", "<color=#00FFAA>[FORTUNE]</color> Your infant body absorbed the rich celestial herbs! A warm energy tempered your baby bones (+10 Tempering, +5 Fitness)!");
                        }
                        else
                        {
                            soldier.health = Mathf.Max(30f, soldier.health - 15f);
                            soldier.meridianDamage += 5f;
                            GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", "<color=#FF6600>[RECOIL]</color> The herbal power was too intense for your infant meridians, causing a feverish night (Health -15, Meridian Damage +5).");
                        }
                    },
                    () =>
                    {
                        soldier.parentRelationship = Mathf.Min(100f, soldier.parentRelationship + 10f);
                        soldier.intellect += 4f;
                        GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", "Mother rushes in, praising your honesty and spoon-feeding you sweet honey broth (+10 Parent Relation, +4 Intellect).");
                    },
                    () =>
                    {
                        soldier.intellect += 5f;
                        soldier.meritRating += 4f;
                        GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", "You cleaned the mess quietly like a disciplined soldier (+5 Intellect, +4 Merit).");
                    }
                );
            }
            else if (Mathf.Approximately(age, 4.0f))
            {
                // Age 4.0: Sibling Warship Dispute
                GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                    "SIBLING CONFLICT",
                    $"The Wooden Warship Dispute: {soldier.siblingRivalName}",
                    $"Your brother {soldier.siblingRivalName} snatches your handcrafted imperial warship model and refuses to give it back!",
                    "Challenge him to a duel with wooden swords! (+Combat, +Clan Standing)",
                    "Negotiate a swap for his wooden catapult toy (+Intellect)",
                    "Complain to Mother (+Family Favor, -Sibling Relation)",
                    () =>
                    {
                        soldier.combatSkill += 4f;
                        soldier.clanSuccessionScore += 5;
                        soldier.siblingRelationship = Mathf.Max(0f, soldier.siblingRelationship - 5f);
                        GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", $"You decisively knocked the toy from {soldier.siblingRivalName}'s grip with a clean wooden swipe! (+4 Combat, +5 Clan Score).");
                    },
                    () =>
                    {
                        soldier.intellect += 6f;
                        soldier.siblingRelationship = Mathf.Min(100f, soldier.siblingRelationship + 8f);
                        GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", "You negotiated a win-win swap, earning your brother's grudging respect (+6 Intellect, +8 Sibling Bond).");
                    },
                    () =>
                    {
                        soldier.parentRelationship = Mathf.Min(100f, soldier.parentRelationship + 6f);
                        soldier.siblingRelationship = Mathf.Max(0f, soldier.siblingRelationship - 10f);
                        GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", $"Mother scolded {soldier.siblingRivalName} and returned your warship, but he glares resentfully (-10 Sibling Bond).");
                    }
                );
            }
            else if (Mathf.Approximately(age, 6.0f))
            {
                // Age 6.0: The School Bully
                GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                    "PRIMARY SCHOOL",
                    "The Courtyard Bully",
                    "A husky boy in the academy courtyard shoves you from behind and demands your afternoon meat ration. How do you respond?",
                    "Fight back fiercely with bare hands! (+Combat, +Fitness)",
                    "Use clever words and outmaneuver him (+Intellect)",
                    "Report him to the martial proctor (+Merit)",
                    () => { soldier.combatSkill += 5f; soldier.fitness += 4f; soldier.health = Mathf.Max(30f, soldier.health - 5f); },
                    () => { soldier.intellect += 6f; soldier.squadRespect += 5f; },
                    () => { soldier.meritRating += 8f; }
                );
            }
            else if (Mathf.Approximately(age, 7.0f))
            {
                // Age 7.0: The Elementary Cheating Ring
                GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                    "ACADEMY SCANDAL",
                    "The Exam Answers Black Market",
                    "A group of older wealthy classmates offers you $80 pocket money if you slip them answers to tomorrow's Imperial Tactics and Math test.",
                    "Accept the $80 and sell the answers (+Cash, -Merit)",
                    "Refuse coldly and focus on your own study (+Intellect, +Merit)",
                    "Report the cheating ring to the Academy Headmaster (+Merit, +Clan Favor)",
                    () =>
                    {
                        soldier.cashBalance += 80;
                        soldier.meritRating = Mathf.Max(0f, soldier.meritRating - 10f);
                        GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", "You pocketed $80 in secret! But the guilt weighs slightly on your martial honor (+$80, -10 Merit).");
                    },
                    () =>
                    {
                        soldier.intellect += 6f;
                        soldier.meritRating += 5f;
                        GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", "You aced the exam on pure merit. Honor above easy gold (+6 Intellect, +5 Merit)!");
                    },
                    () =>
                    {
                        soldier.meritRating += 12f;
                        soldier.clanFavor += 6f;
                        soldier.squadRespect = Mathf.Max(0f, soldier.squadRespect - 5f);
                        GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", "The Headmaster praised your integrity before the entire assembly (+12 Merit, +6 Clan Favor)!");
                    }
                );
            }
            else if (Mathf.Approximately(age, 9.0f))
            {
                // Age 9.0: Infiltrating the Forbidden Clan Martial Pavilion
                GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                    "CLAN SECRETS",
                    "The Unlocked Martial Pavilion",
                    "Passing through the clan courtyard at midnight, you notice the heavy iron gate to the Ancestral Martial Archive was left unlocked by an elder!",
                    "Sneak inside and memorize an ancient meridian breathing chart (+15 Tempering)",
                    "Respect clan tradition and meditate peacefully outside (+5 Fitness, +5 Intellect)",
                    "Lock the door and report it to Father (+10 Parent Relation, +8 Clan Favor)",
                    () =>
                    {
                        soldier.foundationalTempering += 15f;
                        soldier.knowsAboutCultivation = true;
                        GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", "<color=#FFD700>[SECRET UNLOCKED]</color> You studied ancient illuminated silk scrolls of energy circulation! (+15 Foundational Tempering, Cultivation Knowledge Unlocked)!");
                    },
                    () =>
                    {
                        soldier.fitness += 5f;
                        soldier.intellect += 5f;
                        GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", "You sat in peaceful courtyard meditation under the moonlight, refining your focus (+5 Fitness, +5 Intellect).");
                    },
                    () =>
                    {
                        soldier.parentRelationship = Mathf.Min(100f, soldier.parentRelationship + 10f);
                        soldier.clanFavor += 8f;
                        soldier.meritRating += 6f;
                        GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", "Father pats your shoulder warmly: 'Vigilance is the first mark of a true warrior.' (+10 Parent Relation, +8 Clan Favor).");
                    }
                );
            }
            else if (Mathf.Approximately(age, 11.0f))
            {
                // Age 11.0: Sibling Rivalry
                GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                    "CLAN RIVALRY",
                    $"Sibling Rivalry: {soldier.siblingRivalName}'s Challenge",
                    $"Your competitive sibling {soldier.siblingRivalName} boasts that Father favors them for the future clan succession and challenges you to a courtyard duel!",
                    "Accept the challenge and spar with full force! (+Combat, +Clan Score)",
                    "Propose a tactical strategy game instead (+Intellect, +Clan Favor)",
                    "Humbly offer to train together as comrades (+Sibling Bond)",
                    () => { soldier.combatSkill += 6f; soldier.clanSuccessionScore += 10; soldier.siblingRelationship -= 5f; },
                    () => { soldier.intellect += 7f; soldier.clanFavor += 6f; },
                    () => { soldier.siblingRelationship += 15f; soldier.clanFavor += 5f; }
                );
            }
            else if (Mathf.Approximately(age, 12.0f))
            {
                // Age 12.0: Cadet Squad Leader Election
                GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                    "CADET ACADEMY",
                    "Cadet Squad Leader Election",
                    "The cadet instructor announces elections for Squad Leader. Winning grants leadership merit and authority over 10 peers.",
                    "Deliver an impassioned patriotic speech on brotherhood (+Squad Respect, +Merit)",
                    "Intimidate your peers by setting a new cadet pull-up record (+Fitness, +Combat)",
                    "Form a backroom voting pact with the tech cadets (+Intellect, +Clan Favor)",
                    () =>
                    {
                        soldier.squadRespect = Mathf.Min(100f, soldier.squadRespect + 12f);
                        soldier.meritRating += 8f;
                        GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", "Your rousing speech won the squad's unanimous vote! You were appointed Squad Leader (+12 Respect, +8 Merit)!");
                    },
                    () =>
                    {
                        soldier.fitness += 7f;
                        soldier.combatSkill += 5f;
                        soldier.squadRespect = Mathf.Min(100f, soldier.squadRespect + 6f);
                        GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", "Your raw physical dominance left the squad in awe! You were elected Squad Leader (+7 Fitness, +5 Combat)!");
                    },
                    () =>
                    {
                        soldier.intellect += 8f;
                        soldier.clanFavor += 6f;
                        GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", "Your tactical voting coalition carried the day with strategic precision (+8 Intellect, +6 Clan Favor)!");
                    }
                );
            }
            else if (Mathf.Approximately(age, 13.5f))
            {
                // Age 13.5: Cadet School Romance
                GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                    "YOUTHFUL ROMANCE",
                    $"Cadet Crush: {soldier.childhoodRomanceInterest}",
                    $"Fellow cadet {soldier.childhoodRomanceInterest} lingers near your locker after rifle practice and offers you half of their sweet bean pastry.",
                    $"Share the pastry and invite them to rifle practice (+Romance, +Combat)",
                    "Write a tactically worded romantic letter (+Intellect, +Romance)",
                    "Decline politely; focus 100% of your energy on the National Exam (+Fitness, +Merit)",
                    () =>
                    {
                        soldier.combatSkill += 5f;
                        soldier.health = Mathf.Min(100f, soldier.health + 4f);
                        GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", $"You and {soldier.childhoodRomanceInterest} spent the afternoon shooting targets together. A heartwarming cadet bond forms (+5 Combat, +4 Health)!");
                    },
                    () =>
                    {
                        soldier.intellect += 6f;
                        soldier.looks = Mathf.Min(100f, soldier.looks + 4f);
                        GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", $"{soldier.childhoodRomanceInterest} blushed upon reading your letter, tucking it inside their uniform breast pocket (+6 Intellect, +4 Looks)!");
                    },
                    () =>
                    {
                        soldier.fitness += 6f;
                        soldier.meritRating += 6f;
                        GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", "You set your jaw firmly. The Empire's frontline demands total devotion, not sweet pastries (+6 Fitness, +6 Merit)!");
                    }
                );
            }
            else if (Mathf.Approximately(age, 14.5f))
            {
                // Age 14.5: Secret Training Opportunity
                GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                    "MARTIAL TRAINING",
                    "The Wandering Veteran's Advice",
                    "An old, scarred imperial veteran passing through your district offers to teach you a secret military breathing form for a few coins.",
                    "Pay $100 and study the breathing technique diligently (+15 Tempering)",
                    "Decline and stick to official academy curriculum (+5 Merit, +4 Fitness)",
                    "Ask him war stories about the Iron Federation (+8 Intellect)",
                    () => { soldier.cashBalance = Math.Max(0, soldier.cashBalance - 100); soldier.foundationalTempering += 15f; soldier.knowsAboutCultivation = true; },
                    () => { soldier.meritRating += 5f; soldier.fitness += 4f; },
                    () => { soldier.intellect += 8f; }
                );
            }
            else if (Mathf.Approximately(age, 15.0f))
            {
                // Age 15.0: The Underground Sparring Ring
                GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                    "UNDERGROUND COMBAT",
                    "The Back-Alley Sparring Ring",
                    "Older cadets sneak you into an underground warehouse where unlicensed bare-knuckle fighting bouts take place. The purse for tonight's rookie match is $250!",
                    "Step into the ring and fight for the $250 purse! (High Combat/Cash vs Injury)",
                    "Place a $50 wager on the local favorite (+Cash Gamble)",
                    "Walk away; an imperial cadet does not associate with brawlers (+Merit)",
                    () =>
                    {
                        soldier.participatedUndergroundRing = true;
                        float winChance = (soldier.combatSkill * 0.5f) + (soldier.fitness * 0.3f) + (soldier.foundationalTempering * 0.2f);
                        if (UnityEngine.Random.Range(0f, 100f) < winChance)
                        {
                            soldier.cashBalance += 250;
                            soldier.combatSkill += 10f;
                            soldier.squadRespect = Mathf.Min(100f, soldier.squadRespect + 15f);
                            GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", "<color=#00FFAA>[VICTORY!]</color> You knocked out a burly apprentice blacksmith in Round 2! The crowd roared (+$250 Purse, +10 Combat, +15 Respect)!");
                        }
                        else
                        {
                            soldier.health = Mathf.Max(20f, soldier.health - 25f);
                            soldier.combatSkill += 6f;
                            GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", "<color=#FF4444>[DEFEAT]</color> You took heavy blows from an experienced street brawler before going down (Health -25, +6 Combat experience).");
                        }
                    },
                    () =>
                    {
                        if (soldier.cashBalance >= 50)
                        {
                            soldier.cashBalance -= 50;
                            if (UnityEngine.Random.value < 0.55f)
                            {
                                soldier.cashBalance += 120;
                                GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", "Your chosen fighter scored a clean knockout! You won +$120 from the bookmaker!");
                            }
                            else
                            {
                                GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", "Your fighter was outclassed. You lost your $50 bet.");
                            }
                        }
                        else
                        {
                            GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", "You didn't have enough pocket cash to place a wager.");
                        }
                    },
                    () =>
                    {
                        soldier.meritRating += 8f;
                        soldier.parentRelationship = Mathf.Min(100f, soldier.parentRelationship + 6f);
                        GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", "You left the warehouse with your head held high, maintaining the honor of the uniform (+8 Merit).");
                    }
                );
            }
            else if (Mathf.Approximately(age, 16.0f))
            {
                // Age 16.0: Air-Raid Drone Alert
                GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                    "AIR-RAID EMERGENCY",
                    "Iron Federation Scout Drone Breach",
                    "Loud klaxons wail across the district! Three Iron Federation recon drones have breached the border air defense sector and are scanning civilian quarters!",
                    "Sprint to the cadet anti-air battery and aim the interceptor turret (+Combat, +Merit)",
                    "Guide terrified school children to the underground bomb shelter (+Merit, +Parent Relation)",
                    "Scout the perimeter after artillery shoots them down to salvage drone cyber-cores (+Intellect, +$150 Scrap)",
                    () =>
                    {
                        soldier.combatSkill += 8f;
                        soldier.meritRating += 10f;
                        soldier.squadRespect = Mathf.Min(100f, soldier.squadRespect + 8f);
                        GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", "<color=#00FFAA>[SPLASH ONE!]</color> You manned the twin-linked energy gun and vaporized an enemy drone! The District Commander commended your bravery (+8 Combat, +10 Merit)!");
                    },
                    () =>
                    {
                        soldier.meritRating += 10f;
                        soldier.parentRelationship = Mathf.Min(100f, soldier.parentRelationship + 10f);
                        soldier.clanFavor += 6f;
                        GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", "Your cool-headed leadership prevented a stampede, safely escorting 40 children into the shelter (+10 Merit, +10 Family Relation)!");
                    },
                    () =>
                    {
                        soldier.intellect += 8f;
                        soldier.cashBalance += 150;
                        soldier.isTechnicalSchoolGraduate = true;
                        GameEvents.OnFeedEventLogged?.Invoke("DILEMMA", "You salvaged an intact nanite processing chip from the wreckage and sold it to the academy quartermaster (+8 Intellect, +$150, Cyber Affinity Unlocked)!");
                    }
                );
            }
            else if (Mathf.Approximately(age, 17.0f))
            {
                // Age 17.0: Final Exam Specialization
                GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                    "NATIONAL EXAM FOCUS",
                    "Final Preparation Focus",
                    "With the National Military Ranking Exam approaching, where will you dedicate your final semester of preparation?",
                    "Intensive High-Gravity Bootcamp (+12 Fitness, +8 Combat)",
                    "War Room Strategy & Cyber Logistics (+15 Intellect, Cyber Track)",
                    "Meditation & Foundational Tempering (+15 Tempering, +5 All Stats)",
                    () => { soldier.fitness += 12f; soldier.combatSkill += 8f; },
                    () => { soldier.intellect += 15f; soldier.isTechnicalSchoolGraduate = true; },
                    () => { soldier.foundationalTempering += 15f; soldier.fitness += 5f; soldier.intellect += 5f; }
                );
            }
            else if (Mathf.Approximately(age, 17.5f))
            {
                // Age 17.5: The National Mock Exam & "Run to Father for Help"
                TriggerAge17Point5MockExamDilemma(soldier);
            }
        }

        private static void TriggerAge17Point5MockExamDilemma(SoldierProfile soldier)
        {
            if (soldier == null) return;

            int mockScore = soldier.mockExamScore;
            string projectedAcademy = soldier.mockExamProjectedAcademy;

            bool isNoble = soldier.nobleTitle > NobleRank.Commoner;
            bool isWealthyCommoner = !isNoble && soldier.cashBalance >= 300;

            string choiceALabel;
            if (isNoble)
            {
                choiceALabel = "Run to Father for Help (Mobilize Clan Tutors & Lineage Backing)";
            }
            else if (isWealthyCommoner)
            {
                choiceALabel = "Run to Father for Help (Invest $300 Family Savings in Elite Tutors)";
            }
            else
            {
                choiceALabel = "Run to Father for Help (Plead for Family Guidance & Blessing)";
            }

            string choiceBLabel = "Solo Midnight Oil: Library Cramming & Night Ruck Drills (+10 Prep, -10 HP)";
            string choiceCLabel = "Breathing Cultivation: Inner Serenity & Meridian Tempering (+6 Prep, +10 HP)";

            GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                "NATIONAL MOCK EXAM",
                $"Mock Exam: {mockScore}/100 - Final Countdown",
                $"You received your preliminary National Mock Exam results: <b>{mockScore}/100</b> ({projectedAcademy}).\n\n" +
                $"Wealthy and noble peers are running to their fathers to hire elite tutors and pull strings for the final exam. " +
                $"You have 6 months left before the final National Military Ranking Exam at Age 18.0. What will you do?",
                choiceALabel,
                choiceBLabel,
                choiceCLabel,
                // Action A: Run to Father
                () =>
                {
                    soldier.receivedFatherExamBoost = true;
                    if (isNoble)
                    {
                        int boost = (soldier.nobleTitle >= NobleRank.Viscount) ? 18 : 14;
                        soldier.examPreparationBonus += boost;
                        soldier.intellect = Mathf.Min(100f, soldier.intellect + 8f);
                        soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 8f);
                        soldier.clanFavor = Mathf.Min(100f, soldier.clanFavor + 10f);
                        soldier.parentRelationship = Mathf.Min(100f, soldier.parentRelationship + 15f);

                        GameEvents.OnFeedEventLogged?.Invoke("FATHER'S AID",
                            $"<color=#FFD700><b>[FATHER'S NOBLE INTERVENTION]</b></color> Seeing your mock score of {mockScore}/100, your noble father mobilized clan wealth and hired Imperial Academy tutors! " +
                            $"You mastered secret exam strategy scenarios. Final Exam Score Boost: <b>+{boost} points</b>! (Intellect +8, Combat +8, Father Relation +15).");
                    }
                    else if (isWealthyCommoner)
                    {
                        soldier.cashBalance -= 300;
                        int boost = 12;
                        soldier.examPreparationBonus += boost;
                        soldier.intellect = Mathf.Min(100f, soldier.intellect + 7f);
                        soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 6f);
                        soldier.parentRelationship = Mathf.Min(100f, soldier.parentRelationship + 10f);

                        GameEvents.OnFeedEventLogged?.Invoke("FATHER'S AID",
                            $"<color=#FFD700><b>[FATHER'S INVESTMENT]</b></color> You showed Father your mock score ({mockScore}/100). Father withdrew $300 of family savings to hire a veteran imperial drill master! " +
                            $"Final Exam Score Boost: <b>+{boost} points</b>! (Intellect +7, Combat +6).");
                    }
                    else
                    {
                        int boost = 6;
                        soldier.examPreparationBonus += boost;
                        soldier.parentRelationship = Mathf.Min(100f, soldier.parentRelationship + 20f);
                        soldier.meritRating = Mathf.Min(100f, soldier.meritRating + 10f);
                        soldier.cashBalance += 50;

                        GameEvents.OnFeedEventLogged?.Invoke("FATHER'S AID",
                            $"<color=#00FFAA><b>[FATHER'S BLESSING & SACRIFICE]</b></color> You showed Father your mock score ({mockScore}/100). " +
                            $"Tears in his eyes, he embraced you: <i>'Son, we don't have noble gold for tutors, but our clan has grit! Make us proud.'</i> " +
                            $"He slipped you $50 of emergency savings. Final Exam Score Boost: <b>+{boost} points</b>! (+10 Merit, +$50, +20 Father Relation).");
                    }
                },
                // Action B: Solo Midnight Oil
                () =>
                {
                    soldier.examPreparationBonus += 10;
                    soldier.intellect = Mathf.Min(100f, soldier.intellect + 10f);
                    soldier.fitness = Mathf.Min(100f, soldier.fitness + 8f);
                    soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 5f);
                    soldier.health = Mathf.Max(25f, soldier.health - 10f);

                    GameEvents.OnFeedEventLogged?.Invoke("SOLO GRIT",
                        $"<color=#00D4FF><b>[MIDNIGHT CRAMMING & NIGHT DRILLS]</b></color> You relied on pure personal determination, studying tactics by candlelight and doing midnight ruck marches. " +
                        $"Final Exam Score Boost: <b>+10 points</b>! (Intellect +10, Fitness +8, Combat +5, Health -10).");
                },
                // Action C: Breathing Cultivation & Mental Composure
                () =>
                {
                    soldier.examPreparationBonus += 6;
                    soldier.foundationalTempering = Mathf.Min(100f, soldier.foundationalTempering + 15f);
                    soldier.health = Mathf.Min(100f, soldier.health + 10f);
                    soldier.fitness = Mathf.Min(100f, soldier.fitness + 4f);

                    GameEvents.OnFeedEventLogged?.Invoke("INNER SERENITY",
                        $"<color=#00FFAA><b>[COMPOSURE & MERIDIAN TEMPERING]</b></color> You focused on mental stillness and foundational breath regulation, entering exam season with unshakeable calm. " +
                        $"Final Exam Score Boost: <b>+6 points</b>! (Foundational Tempering +15, Health +10).");
                }
            );
        }

        public static int CalculateProjectedExamScore(SoldierProfile soldier, out string projectedAcademy)
        {
            if (soldier == null)
            {
                projectedAcademy = "Recruit Depot";
                return 220;
            }

            // 750-Point National Ranking Exam Scaling (3 stages x 250 max)
            // Baseline 3 trials projection (each up to ~250 pts)
            float physicalScore = ((soldier.fitness * 0.5f) + (soldier.combatSkill * 0.5f)) * 2.2f + 25f;
            float tacticalScore = (soldier.intellect * 2.0f) + 30f;
            float martialScore = ((soldier.combatSkill * 0.6f) + (soldier.foundationalTempering * 0.4f)) * 2.0f + 20f;

            float nobleBonus = (soldier.nobleTitle > NobleRank.Commoner) ? ((int)soldier.nobleTitle * 8f) : 0f;
            float prepBonus = soldier.examPreparationBonus * 2.0f;
            float primaryBonus = (soldier.primarySchoolScore - 50f) * 0.4f;

            float tierBonus = 0f;
            switch (soldier.schoolTier)
            {
                case SchoolTier.City3rd: tierBonus = 0f; break;
                case SchoolTier.City2nd: tierBonus = 12f; break;
                case SchoolTier.City1st: tierBonus = 24f; break;
                case SchoolTier.State3rd: tierBonus = 40f; break;
                case SchoolTier.State2nd: tierBonus = 60f; break;
                case SchoolTier.State1st: tierBonus = 80f; break;
                case SchoolTier.ImperialPinnacle: tierBonus = 105f; break;
            }
            if (soldier.isImperialScholarshipStudent) tierBonus += 20f;

            int score = Mathf.Clamp(Mathf.RoundToInt(physicalScore + tacticalScore + martialScore + nobleBonus + prepBonus + primaryBonus + tierBonus), 150, 750);

            // Placement benchmarks across 750 points:
            // City students difficult to cross 50% (< 375 pts = Frontline Recruit Depot)
            // 50% - 74.9% (375 - 559 pts) = Technical & Cyber Warfare School (E-3 PFC)
            // State students difficult to score 75% (560 - 637 pts) = Vanguard Candidate Academy (E-4 Specialist)
            // 85% - 94.9% (638 - 711 pts) = Imperial War Academy Honors (E-4 Fast-Track)
            // >= 95% (>= 712 pts) = Imperial Crown War Academy (Officer O-1 2nd Lieutenant)
            if (score >= 712)
            {
                projectedAcademy = "Imperial Crown War Academy (Officer O-1 Elite)";
            }
            else if (score >= 638)
            {
                projectedAcademy = "Imperial War Academy Honors (E-4 Fast-Track)";
            }
            else if (score >= 560)
            {
                projectedAcademy = "Vanguard Candidate Academy (E-4 Specialist)";
            }
            else if (score >= 375)
            {
                projectedAcademy = "Technical & Cyber Warfare School (E-3 PFC)";
            }
            else
            {
                projectedAcademy = "Frontline Recruit Depot (E-1 Grunt)";
            }

            return score;
        }

        public static void ConductMockExam(SoldierProfile soldier)
        {
            if (soldier == null) return;
            soldier.mockExamScore = CalculateProjectedExamScore(soldier, out string projectedAcademy);
            soldier.tookMockExam = true;
            soldier.mockExamProjectedAcademy = projectedAcademy;

            GameEvents.OnFeedEventLogged?.Invoke("MOCK EXAM",
                $"<color=#00FFAA><b>[NATIONAL MOCK EXAM RESULT]</b></color>\n" +
                $"Preliminary Score: <b>{soldier.mockExamScore} / 750</b>\n" +
                $"Projected Placement: <b>{projectedAcademy}</b>\n" +
                $"Current Prep Bonus: <b>+{soldier.examPreparationBonus * 2.0f:F0} pts</b>. Run to father or study before the final exam at Age 18!");
        }

        public static void RequestFatherExamIntervention(SoldierProfile soldier)
        {
            if (soldier == null) return;

            bool isNoble = soldier.nobleTitle > NobleRank.Commoner;
            bool isWealthyCommoner = !isNoble && soldier.cashBalance >= 300;

            if (isNoble)
            {
                int nobleBoost = (int)soldier.nobleTitle * 6 + 6;
                soldier.examPreparationBonus += nobleBoost;
                soldier.parentRelationship = Mathf.Min(100f, soldier.parentRelationship + 10f);
                soldier.intellect = Mathf.Min(100f, soldier.intellect + 8f);
                soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 6f);

                GameEvents.OnFeedEventLogged?.Invoke("FAMILY BACKING",
                    $"<color=#FFD700><b>[FATHER DEPLOYS NOBLE RESOURCES]</b></color> Father nods proudly upon seeing your mock score: " +
                    $"<i>'A son of our House must graduate at the pinnacle.'</i> He summons imperial tutors and ancient exam manuscripts! " +
                    $"Exam Score Boost: <b>+{nobleBoost} points</b>! (Intellect +8, Combat +6, Parent Relation +10).");
            }
            else if (isWealthyCommoner)
            {
                soldier.cashBalance -= 300;
                soldier.examPreparationBonus += 12;
                soldier.intellect = Mathf.Min(100f, soldier.intellect + 6f);
                soldier.parentRelationship = Mathf.Min(100f, soldier.parentRelationship + 6f);

                GameEvents.OnFeedEventLogged?.Invoke("FAMILY SACRIFICE",
                    $"<color=#00D4FF><b>[FATHER INVESTS SAVINGS]</b></color> Father breaks open the family ceramic vault ($300): " +
                    $"<i>'Take this. Hire the retired academy sergeant for private coaching.'</i> " +
                    $"Exam Score Boost: <b>+12 points</b>! (Intellect +6, -$300 Family Savings).");
            }
            else
            {
                soldier.examPreparationBonus += 6;
                soldier.parentRelationship = Mathf.Min(100f, soldier.parentRelationship + 12f);
                soldier.fitness = Mathf.Min(100f, soldier.fitness + 4f);

                GameEvents.OnFeedEventLogged?.Invoke("FAMILY BLESSING",
                    $"<color=#00FFAA><b>[FATHER'S HAND ON YOUR SHOULDER]</b></color> Father sighs warmly: " +
                    $"<i>'We don't have gold or noble seals, son. But you have our blood and endurance. Stand tall.'</i> " +
                    $"Exam Score Boost: <b>+6 points</b>! (Parent Relation +12, Fitness +4).");
            }
        }

        // ==================== 3-STAGE INTERACTIVE NATIONAL MILITARY RANKING EXAM (AGE 18) ====================

        /// <summary>
        /// Conducts the National Military Ranking Exam at Age 18.0 as an interactive 3-stage trial!
        /// Stage 1: High-Gravity Obstacle Course & Trench Run (Physical)
        /// Stage 2: Holographic War Room Simulation (Tactical)
        /// Stage 3: Live Proctor Sparring Duel (Martial)
        /// </summary>
        public static void ConductNationalMilitaryExam(SoldierProfile soldier, GameManager gm)
        {
            if (soldier == null || soldier.currentRank != null) return;

            GameEvents.OnFeedEventLogged?.Invoke("EXAM DAY",
                $"<color=#FFD700>==================================================\n" +
                $"<b>[THE NATIONAL MILITARY RANKING EXAM • AGE 18.0]</b>\n" +
                $"The day has arrived! Over 200,000 aspiring candidates across the Empire assemble under the watchful eyes of Imperial Marshals and Academy Proctors.\n" +
                $"The 3-stage trials will test your body, your tactical intellect, and your martial steel!\n" +
                $"==================================================</color>");

            // Start Stage 1: The High-Gravity Obstacle Course
            TriggerExamStage1(soldier, gm);
        }

        private static void TriggerExamStage1(SoldierProfile soldier, GameManager gm)
        {
            if (GameEvents.OnChildhoodDilemmaTriggered == null)
            {
                // Headless test / automated simulation fallback when no UI is listening
                float physicalPerf = (soldier.fitness * 0.5f) + (soldier.foundationalTempering * 0.5f);
                soldier.examPhysicalScore = Mathf.RoundToInt(Mathf.Clamp(physicalPerf * 2.2f + 25f, 90f, 250f));

                float tacticalPerf = (soldier.intellect * 0.5f) + (soldier.combatSkill * 0.5f);
                soldier.examTacticalScore = Mathf.RoundToInt(Mathf.Clamp(tacticalPerf * 2.2f + 25f, 90f, 250f));

                float martialPerf = (soldier.combatSkill * 0.50f) + (soldier.powerTier * 15f) + (soldier.talentScore * 0.25f);
                soldier.examMartialScore = Mathf.RoundToInt(Mathf.Clamp(martialPerf * 1.8f + 25f, 90f, 250f));

                FinalizeExamAndCommission(soldier, gm);
                return;
            }

            GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                "EXAM STAGE 1: PHYSICAL TRIAL",
                "The 5km High-Gravity Trench Run (250 pts)",
                "The trial horn blares! You are thrown onto the brutal 5km high-gravity obstacle course: barbed wire crawls, electrified mud pits, and concrete climbing walls under simulated artillery fire.",
                "Activate Meridian Breathing & Full Sprint (High stamina & tempering test)",
                "Paced Tactical Cadence (Safe, steady energy management)",
                "Push Past the Physical Limit with Raw Grit (+Score, Costs 15 HP)",
                // Choice A: Meridian Breathing Sprint
                () =>
                {
                    float performance = (soldier.fitness * 0.5f) + (soldier.foundationalTempering * 0.5f);
                    int stagePoints = Mathf.RoundToInt(Mathf.Clamp(performance * 2.2f + 25f, 90f, 250f));
                    soldier.examPhysicalScore = stagePoints;

                    GameEvents.OnFeedEventLogged?.Invoke("EXAM TRIAL",
                        $"<b>[STAGE 1 RESULT]</b> Your meridian breathing surged smoothly! You vaulted the final wall in record time! " +
                        $"Physical Trial Score: <color=#00FFAA><b>+{stagePoints} / 250 pts</b></color>.");

                    TriggerExamStage2(soldier, gm);
                },
                // Choice B: Paced Cadence
                () =>
                {
                    int stagePoints = Mathf.RoundToInt(Mathf.Clamp((soldier.fitness * 1.8f) + 60f, 110f, 215f));
                    soldier.examPhysicalScore = stagePoints;

                    GameEvents.OnFeedEventLogged?.Invoke("EXAM TRIAL",
                        $"<b>[STAGE 1 RESULT]</b> You paced yourself with disciplined cadence, crossing the finish line without a scratch. " +
                        $"Physical Trial Score: <color=#00D4FF><b>+{stagePoints} / 250 pts</b></color>.");

                    TriggerExamStage2(soldier, gm);
                },
                // Choice C: Raw Grit
                () =>
                {
                    soldier.health = Mathf.Max(25f, soldier.health - 15f);
                    int stagePoints = Mathf.RoundToInt(Mathf.Clamp((soldier.fitness * 2.0f) + 45f, 115f, 240f));
                    soldier.examPhysicalScore = stagePoints;

                    GameEvents.OnFeedEventLogged?.Invoke("EXAM TRIAL",
                        $"<b>[STAGE 1 RESULT]</b> You sprinted till your lungs burned, collapsing across the finish line with bleeding knees! " +
                        $"Physical Trial Score: <color=#FFD700><b>+{stagePoints} / 250 pts</b></color> (Health -15).");

                    TriggerExamStage2(soldier, gm);
                }
            );
        }

        private static void TriggerExamStage2(SoldierProfile soldier, GameManager gm)
        {
            GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                "EXAM STAGE 2: WAR ROOM SIMULATION",
                "Sector 9 Mech Incursion Scenario (250 pts)",
                "Inside the holographic war simulation chamber, an Iron Federation armored mech battalion breaks through the defensive perimeter in Sector 9. As candidate commander, what is your order?",
                "Aggressive Flanking Ambush (High risk, tests Intellect & Combat Aptitude)",
                "Defense-in-Depth & Funnel Artillery Strike (Textbook tactical mastery)",
                "Cyber-Disruption & EMP Electronic Jamming (Cyber Warfare Track)",
                // Choice A: Flanking Ambush
                () =>
                {
                    float eval = (soldier.intellect * 0.5f) + (soldier.combatSkill * 0.5f);
                    int points = Mathf.RoundToInt(Mathf.Clamp(eval * 2.2f + 25f, 90f, 250f));
                    soldier.examTacticalScore = points;

                    GameEvents.OnFeedEventLogged?.Invoke("EXAM TRIAL",
                        $"<b>[STAGE 2 RESULT]</b> Your daring armored pincer movement encircled and annihilated the enemy vanguard! " +
                        $"Tactical Strategy Score: <color=#00FFAA><b>+{points} / 250 pts</b></color>.");

                    TriggerExamStage3(soldier, gm);
                },
                // Choice B: Defense-in-Depth
                () =>
                {
                    int points = Mathf.RoundToInt(Mathf.Clamp((soldier.intellect * 1.9f) + 55f, 110f, 225f));
                    soldier.examTacticalScore = points;

                    GameEvents.OnFeedEventLogged?.Invoke("EXAM TRIAL",
                        $"<b>[STAGE 2 RESULT]</b> The evaluators nodded in approval at your disciplined artillery funnel corridors! " +
                        $"Tactical Strategy Score: <color=#00D4FF><b>+{points} / 250 pts</b></color>.");

                    TriggerExamStage3(soldier, gm);
                },
                // Choice C: Cyber-Disruption
                () =>
                {
                    soldier.isTechnicalSchoolGraduate = true;
                    int points = Mathf.RoundToInt(Mathf.Clamp((soldier.intellect * 2.0f) + 40f, 115f, 235f));
                    soldier.examTacticalScore = points;

                    GameEvents.OnFeedEventLogged?.Invoke("EXAM TRIAL",
                        $"<b>[STAGE 2 RESULT]</b> You hijacked the simulated enemy comms network and triggered an automated shutdown! " +
                        $"Cyber Strategy Score: <color=#FFD700><b>+{points} / 250 pts</b></color> (Guaranteed Tech Stipend Qualification)!");

                    TriggerExamStage3(soldier, gm);
                }
            );
        }

        private static void TriggerExamStage3(SoldierProfile soldier, GameManager gm)
        {
            GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                "EXAM STAGE 3: PROCTOR DUEL",
                "The Live Martial Appraisal (250 pts)",
                "Final stage! Senior Warrant Officer Proctor Gary—a towering veteran in combat armor wielding a heavy shock-blade—steps into the central ring. 'Show me your steel, candidate!'",
                "Explosive Martial Strike (Direct power test; tests Combat Skill & Cultivation)",
                "Technical Parry & Counter-Takedown (Tests Combat Skill & Intellect)",
                "Iron Defense: Absorb & Outlast (Tests Tempering & Fitness)",
                // Choice A: Explosive Strike
                () =>
                {
                    float martialRating = (soldier.combatSkill * 0.50f) + (soldier.powerTier * 15f) + (soldier.talentScore * 0.25f);
                    int points = Mathf.RoundToInt(Mathf.Clamp(martialRating * 1.8f + 25f, 90f, 250f));
                    soldier.examMartialScore = points;

                    GameEvents.OnFeedEventLogged?.Invoke("EXAM TRIAL",
                        $"<b>[STAGE 3 RESULT]</b> Your shockwave strike forced Proctor Gary back two paces! The evaluator grins: 'Fine power, cadet!' " +
                        $"Martial Duel Score: <color=#00FFAA><b>+{points} / 250 pts</b></color>.");

                    FinalizeExamAndCommission(soldier, gm);
                },
                // Choice B: Technical Parry
                () =>
                {
                    float technicalRating = (soldier.combatSkill * 0.50f) + (soldier.intellect * 0.40f);
                    int points = Mathf.RoundToInt(Mathf.Clamp(technicalRating * 2.1f + 30f, 100f, 235f));
                    soldier.examMartialScore = points;

                    GameEvents.OnFeedEventLogged?.Invoke("EXAM TRIAL",
                        $"<b>[STAGE 3 RESULT]</b> You cleanly deflected his heavy slash and swept his lead leg, demonstrating pristine academy form! " +
                        $"Martial Duel Score: <color=#00D4FF><b>+{points} / 250 pts</b></color>.");

                    FinalizeExamAndCommission(soldier, gm);
                },
                // Choice C: Iron Defense
                () =>
                {
                    float enduranceRating = (soldier.foundationalTempering * 0.55f) + (soldier.fitness * 0.45f);
                    int points = Mathf.RoundToInt(Mathf.Clamp(enduranceRating * 2.0f + 35f, 100f, 230f));
                    soldier.examMartialScore = points;

                    GameEvents.OnFeedEventLogged?.Invoke("EXAM TRIAL",
                        $"<b>[STAGE 3 RESULT]</b> You absorbed Gary's three-hit flurry with unwavering stance, standing firm like an iron bastion! " +
                        $"Martial Duel Score: <color=#FFD700><b>+{points} / 250 pts</b></color>.");

                    FinalizeExamAndCommission(soldier, gm);
                }
            );
        }

        private static void FinalizeExamAndCommission(SoldierProfile soldier, GameManager gm)
        {
            if (soldier == null) return;

            // Compute total score from the 3 trials (max 250 each = 750) + background preparation + lineage backing + primary school score + school tier!
            int stageTotal = soldier.examPhysicalScore + soldier.examTacticalScore + soldier.examMartialScore;
            float nobleBonus = (soldier.nobleTitle > NobleRank.Commoner) ? ((int)soldier.nobleTitle * 8f) : 0f;
            float prepBonus = soldier.examPreparationBonus * 2.0f;
            float primaryBonus = (soldier.primarySchoolScore - 50f) * 0.4f;

            float tierBonus = 0f;
            switch (soldier.schoolTier)
            {
                case SchoolTier.City3rd: tierBonus = 0f; break;
                case SchoolTier.City2nd: tierBonus = 12f; break;
                case SchoolTier.City1st: tierBonus = 24f; break;
                case SchoolTier.State3rd: tierBonus = 40f; break;
                case SchoolTier.State2nd: tierBonus = 60f; break;
                case SchoolTier.State1st: tierBonus = 80f; break;
                case SchoolTier.ImperialPinnacle: tierBonus = 105f; break;
            }
            if (soldier.isImperialScholarshipStudent) tierBonus += 20f;

            soldier.examScore = Mathf.Clamp(Mathf.RoundToInt(stageTotal + nobleBonus + prepBonus + primaryBonus + tierBonus), 150, 750);

            // Determine Commission & Starting Rank based on 750-point scale:
            // City students difficult to cross 50% (< 375 = Frontline Recruit Depot)
            // 50% - 74.9% (375 - 559) = Technical & Cyber Warfare School (E-3 PFC)
            // State students difficult to score 75% (560 - 637) = Vanguard Candidate Academy (E-4 Specialist)
            // 85% - 94.9% (638 - 711) = Imperial War Academy Honors (E-4 Fast-Track OCS Candidate)
            // >= 95% (>= 712) = Imperial Crown War Academy (Officer O-1 2nd Lieutenant)
            MilitaryRankSO assignedRank = null;
            string academyName;
            string department;
            int startingBonusCash = 0;

            if (soldier.examScore >= 712)
            {
                // Top 1% Pinnacle: Imperial Crown War Academy (Officer O-1 2nd Lieutenant)
                academyName = "<color=#FFD700><b>THE IMPERIAL CROWN WAR ACADEMY (TOP 1% ELITE COMMAND)</b></color>";
                department = "Officer Corps & Strategic Command";
                assignedRank = FindRank(gm, "O-1") ?? FindRank(gm, "E-4");
                startingBonusCash = 4000;
            }
            else if (soldier.examScore >= 638)
            {
                // 85-90% State Prodigy & Imperial Honors Stream (E-4 Specialist / Fast-Track)
                academyName = "<color=#FFD700><b>IMPERIAL WAR ACADEMY (HONORS STREAM)</b></color>";
                department = "Vanguard Strike & Tactical Staff";
                assignedRank = FindRank(gm, "E-4") ?? FindRank(gm, "E-3");
                startingBonusCash = 2500;
            }
            else if (soldier.examScore >= 560)
            {
                // Crossing 75%: Vanguard Candidate Academy (E-4 Specialist/Corporal)
                academyName = "<color=#00FFAA><b>VANGUARD CANDIDATE ACADEMY</b></color>";
                department = "Specialist & Reconnaissance";
                assignedRank = FindRank(gm, "E-4") ?? FindRank(gm, "E-3");
                startingBonusCash = 1500;
            }
            else if (soldier.examScore >= 375)
            {
                // Crossing 50%: Technical & Cyber Warfare School (E-3 Private First Class)
                academyName = "<color=#00D4FF><b>TECHNICAL & CYBER WARFARE SCHOOL</b></color>";
                department = "Technical & Cyber Operations";
                soldier.isTechnicalSchoolGraduate = true;
                assignedRank = FindRank(gm, "E-3");
                startingBonusCash = 800;
            }
            else
            {
                // Below 50%: Frontline Grunt Walk-In (E-1 Private Recruit)
                academyName = "<color=#AAAAAA><b>VALERIAN GROUND FORCES RECRUIT DEPOT</b></color>";
                department = "Frontline Infantry";
                assignedRank = FindRank(gm, "E-1");
                startingBonusCash = 300;
            }

            if (assignedRank != null)
            {
                soldier.currentRank = assignedRank;
            }
            soldier.assignedDepartment = department;
            soldier.cashBalance += startingBonusCash;

            // Score Breakdown Summary
            string scoreProgression = soldier.tookMockExam
                ? $"\n• Mock Exam (Age 17.5): <b>{soldier.mockExamScore}/750</b> ({soldier.mockExamProjectedAcademy})\n• Final Exam (Age 18.0): <b>{soldier.examScore}/750</b> (<color=#00FFAA>+{(soldier.examScore - soldier.mockExamScore):+0;-0;0} improvement!</color>)"
                : $"\n• Final Exam Score: <b>{soldier.examScore}/750</b>";

            string trialBreakdown = $"\n• Trial Breakdown: Physical (+{soldier.examPhysicalScore}/250) | Tactical (+{soldier.examTacticalScore}/250) | Martial (+{soldier.examMartialScore}/250)";
            if (soldier.examPreparationBonus > 0)
            {
                trialBreakdown += $" | Prep Boost (+{soldier.examPreparationBonus * 2.0f:F0})";
            }
            trialBreakdown += $" | Primary School ({soldier.primarySchoolGraduationHonors}, {(primaryBonus >= 0 ? "+" : "")}{primaryBonus:F1} pts)";
            trialBreakdown += $" | School Tier ({SchoolHelper.GetTierTitle(soldier.schoolTier)}, +{tierBonus:F1} pts)";

            GameEvents.OnFeedEventLogged?.Invoke("COMMISSION",
                $"<color=#FFD700>==================================================\n" +
                $"<b>[OFFICIAL COMMISSION & ENLISTMENT RESULTS • AGE 18.0]</b>" +
                $"{scoreProgression}" +
                $"{trialBreakdown}\n" +
                $"Assigned Academy: {academyName}\n" +
                $"Starting Rank: <b>{(soldier.currentRank != null ? soldier.currentRank.rankName : "Private")} ({(soldier.currentRank != null ? soldier.currentRank.paygrade : "E-1")})</b>\n" +
                $"Branch: <b>{department}</b> | Signing Bonus: <b>+${startingBonusCash:N0}</b>\n" +
                $"==================================================</color>");

            GameEvents.OnFeedEventLogged?.Invoke("ACTIVE DUTY",
                $"<color=#00FFAA><b>[OFFICIAL ENLISTMENT COMPLETE]</b></color> At age 18.0, your childhood in the Valerian Empire has concluded. " +
                $"You put on the combat fatigues of the Valerian Vanguard. Your active military career begins now!");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }

        private static MilitaryRankSO FindRank(GameManager gm, string paygrade)
        {
            if (gm == null || gm.militaryRankDatabase == null) return null;
            foreach (var rank in gm.militaryRankDatabase)
            {
                if (rank != null && rank.paygrade == paygrade) return rank;
            }
            return null;
        }

        // ==================== YOUTH MANUAL ACTIVITIES ====================

        public static void PerformBodyTempering(SoldierProfile soldier)
        {
            if (soldier == null) return;
            soldier.fitness = Mathf.Min(100f, soldier.fitness + 5f);
            soldier.foundationalTempering = Mathf.Min(100f, soldier.foundationalTempering + 7f);
            soldier.health = Mathf.Min(100f, soldier.health + 2f);

            GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                $"<color=#00FFAA>[BODY TEMPERING DRILL]</color> You completed an intense session of body conditioning and meridian breathing! " +
                $"Fitness: <b>{soldier.fitness:F0}/100</b> | Foundational Tempering: <b>{soldier.foundationalTempering:F0}/100</b>.");
            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }

        public static void PerformStudyTactics(SoldierProfile soldier)
        {
            if (soldier == null) return;
            soldier.intellect = Mathf.Min(100f, soldier.intellect + 6f);
            soldier.meritRating = Mathf.Min(100f, soldier.meritRating + 3f);

            GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                $"<color=#00AAFF>[TACTICAL STUDY]</color> You read imperial war archives and analyzed enemy drone strategies! " +
                $"Intellect: <b>{soldier.intellect:F0}/100</b> (Boosts age-18 exam score!).");
            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }

        public static void PerformCadetFiringRange(SoldierProfile soldier)
        {
            if (soldier == null) return;

            if (soldier.age < 10.0f)
            {
                soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 3f);
                GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                    $"<color=#FF8800>[SLINGSHOT PRACTICE]</color> You set up wooden bottles in the courtyard and practiced with a leather sling! (+3 Combat Skill).");
            }
            else
            {
                soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 7f);
                soldier.fitness = Mathf.Min(100f, soldier.fitness + 2f);
                GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                    $"<color=#FF5500>[CADET FIRING RANGE]</color> You spent the afternoon firing energy carbine training rounds at 200m targets! " +
                    $"Combat Skill: <b>{soldier.combatSkill:F0}/100</b> | Marksmanship improved (+7 Combat, +2 Fitness).");
            }
            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }

        public static void PerformClanMartialLibrary(SoldierProfile soldier)
        {
            if (soldier == null) return;

            soldier.intellect = Mathf.Min(100f, soldier.intellect + 5f);
            soldier.foundationalTempering = Mathf.Min(100f, soldier.foundationalTempering + 6f);
            soldier.knowsAboutCultivation = true;

            GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                $"<color=#00D4FF>[CLAN MARTIAL LIBRARY]</color> You unrolled ancient silk scrolls of body tempering and qi meridian flow! " +
                $"Foundational Tempering: <b>{soldier.foundationalTempering:F0}/100</b> | Intellect: <b>{soldier.intellect:F0}/100</b> (Cultivation insight deepened).");
            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }

        public static void PerformStreetErrandsAndJobs(SoldierProfile soldier)
        {
            if (soldier == null) return;

            int earnings = (soldier.age >= 12.0f) ? UnityEngine.Random.Range(40, 85) : UnityEngine.Random.Range(15, 35);
            soldier.cashBalance += earnings;
            soldier.fitness = Mathf.Min(100f, soldier.fitness + 2f);
            soldier.meritRating = Mathf.Min(100f, soldier.meritRating + 2f);

            GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                $"<color=#FFD700>[DISTRICT ODD JOBS]</color> You helped the district armorer carry heavy iron ingots and delivered tactical parcels! " +
                $"Earned: <b>+${earnings}</b> pocket cash! (Fitness +2, Bank: ${soldier.cashBalance:N0}).");
            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }

        public static void PerformApothecaryNourishment(SoldierProfile soldier)
        {
            if (soldier == null) return;

            int soupCost = 30;
            if (soldier.cashBalance >= soupCost)
            {
                soldier.cashBalance -= soupCost;
                soldier.health = Mathf.Min(100f, soldier.health + 8f);
                soldier.foundationalTempering = Mathf.Min(100f, soldier.foundationalTempering + 5f);

                GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                    $"<color=#00FFAA>[APOTHECARY BONE BROTH]</color> You bought a steaming bowl of ginseng and iron-marrow root broth (-${soupCost})! " +
                    $"Health restored to <b>{soldier.health:F0}/100</b> | Tempering: <b>{soldier.foundationalTempering:F0}/100</b>.");
            }
            else
            {
                GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                    $"<color=#FF8800>[INSUFFICIENT FUNDS]</color> A nourishing bowl of apothecary bone soup costs ${soupCost}. You only have ${soldier.cashBalance}.");
            }
            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }

        public static void PerformFamilyBonding(SoldierProfile soldier)
        {
            if (soldier == null) return;
            soldier.parentRelationship = Mathf.Min(100f, soldier.parentRelationship + 8f);
            soldier.clanFavor = Mathf.Min(100f, soldier.clanFavor + 4f);
            soldier.cashBalance += 40;

            if (soldier.age >= 17.0f)
            {
                soldier.examPreparationBonus += 2;
                GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                    $"<color=#FFD700>[FAMILY TIME & EXAM COACHING]</color> You spent the afternoon with Father reviewing past military campaigns. " +
                    $"Father shared veteran exam tips (+2 Exam Prep Bonus)! Parent Relationship: <b>{soldier.parentRelationship:F0}/100</b>. Father gave you <b>+$40</b> pocket money!");
            }
            else
            {
                GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                    $"<color=#FFD700>[FAMILY TIME]</color> You spent the afternoon helping your parents around the household. " +
                    $"Parent Relationship: <b>{soldier.parentRelationship:F0}/100</b>. Father slipped you <b>+$40</b> allowance!");
            }
            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }

        public static void PerformSiblingSparring(SoldierProfile soldier)
        {
            if (soldier == null) return;
            soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 5f);
            soldier.clanSuccessionScore += 5;

            GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                $"<color=#FF8844>[SIBLING SPARRING]</color> You challenged your rival sibling <b>{soldier.siblingRivalName}</b> to a mock duel in the courtyard! " +
                $"Combat Skill: <b>{soldier.combatSkill:F0}/100</b> | Clan Succession Standing: <b>+{soldier.clanSuccessionScore}</b>.");
            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }
    }
}
