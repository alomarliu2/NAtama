﻿// Generated by Xamasoft JSON Class Generator
// http://www.xamasoft.com/json-class-generator

using System;
using System.Collections.Generic;

namespace Model.DBF
{        
    public class SnatchStageLib:DBFItemBase
    {        
        public class GateInfo
        {
            public int NpcID = -1;
            public int Weight = 0;
        }

        public class SkillInfo
        {
            public string Name = "";
            public int BasePercent = 0;
            public int Percent = 0;
            public int Compute = 0;
            public int Flag = 0;
        }

        public class ExpInfo
        {
            public int Win = 0;
            public int Lose = 0;
            public int Tie = 0;
        }

        /// <summary>名稱</summary>
        public string NAME = "";
        /// <summary>基礎攻擊</summary>
        public int BaseATK = 0;
        /// <summary>每一級攻擊提升</summary>
        public int GrowingATK = 0;
        /// <summary>升級所需道具</summary>
        public int[] Require = null;
        
        /// <summary>技能資訊</summary>
        public SkillInfo Skill = null;

        /// <summary>通關經驗值資訊</summary>
        public ExpInfo Exp;
        
        public int GetGameExp(int result)
        {
            int exp = 0;

            switch(result)
            {
                case 0:
                    exp = Exp.Lose;
                    break;
                case 1:
                    exp = Exp.Win;
                    break;
                case 2:
                    exp = Exp.Tie;
                    break;
            }

            return exp;
        }
    }
}
