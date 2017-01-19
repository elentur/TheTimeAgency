using System.Collections.Generic;

    public class Person
    {
        public string name;

        public bool isOffender = false;
        public bool isVictim = false;

        public List<Evidence> evidences = new List<Evidence>();

        public bool checkAllEvidences()
        {
            foreach(Evidence e in evidences)
            {
                if (e.person != null && e.person != name) return false;
            }
            return true;
        }
    }
