using System;

namespace ArmyLife.Data
{
    public enum VeteranCareer
    {
        None,                   // Just retired / living on pension
        UniversityGI,           // Attending university on the GI Bill ($500/mo stipend)
        DefenseContractor,      // Private security or arms procurement ($2,500/mo)
        CityMayor,              // Elected Mayor of your hometown ($4,000/mo, requires 500+ Merit/Medals)
        RegionalGovernor,       // Elected Provincial Governor ($8,000/mo, requires Colonel/General)
        MinisterOfDefense       // Appointed National Minister of Defense ($18,000/mo, Supreme Allied Commander)
    }

    [Serializable]
    public class ChildHeir
    {
        public string childName;
        public string gender;
        public float age;
        public float relationship = 80f;
        public bool hasPrivateTutors = false;
        public int examScoreBonus = 0;
        public bool isEligibleForEnlistment => age >= 18.0f;

        public ChildHeir() {}

        public ChildHeir(string name, string gender, float startingAge = 1.0f)
        {
            this.childName = name;
            this.gender = gender;
            this.age = startingAge;
            this.relationship = 80f;
        }
    }
}
