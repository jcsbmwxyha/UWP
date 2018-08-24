using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraEditor.Common
{
    static class Definitions
    {
        static public int GetSecondsPerTimeUnitByLevel(int level)
        {
            int seconds;

            if (level == 1) seconds = 1;
            else if (level == 2) seconds = 2;
            else if (level == 3) seconds = 5;
            else if (level == 4) seconds = 15;
            else if (level == 5) seconds = 30;
            else seconds = 60;

            return seconds;
        }
        public const int GridWidthPixels = 35;
    }                                               
}                                                   
                                                    
                                                    
                                                    
                                                    