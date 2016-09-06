using System;

namespace CritterShell.Critters
{
    public class CritterWithMergeableProperties
    {
        protected Activity MergeActivity(Activity thisActivity, Activity otherActivity)
        {
            if (thisActivity == otherActivity)
            {
                return thisActivity;
            }

            return thisActivity | otherActivity;
        }

        protected Age MergeAge(Age thisAge, Age otherAge)
        {
            if (thisAge == otherAge)
            {
                return thisAge;
            }

            return Age.Various;
        }

        protected Confidence MergeConfidence(Confidence thisConfidence, Confidence otherConfidence)
        {
            if (thisConfidence == otherConfidence)
            {
                return thisConfidence;
            }

            return (Confidence)Math.Min((int)thisConfidence, (int)otherConfidence);
        }

        protected GroupType MergeGroupType(GroupType thisGroupType, GroupType otherGroupType)
        {
            if (thisGroupType == otherGroupType)
            {
                return thisGroupType;
            }

            // promote to largest group type
            if (thisGroupType == GroupType.Group || otherGroupType == GroupType.Group)
            {
                return GroupType.Group;
            }
            if (thisGroupType == GroupType.Family || otherGroupType == GroupType.Family)
            {
                return GroupType.Family;
            }
            if (thisGroupType == GroupType.Pair || otherGroupType == GroupType.Pair)
            {
                return GroupType.Pair;
            }
            if (thisGroupType == GroupType.Single || otherGroupType == GroupType.Single)
            {
                return GroupType.Single;
            }
            if (thisGroupType == GroupType.NotApplicable || otherGroupType == GroupType.NotApplicable)
            {
                return GroupType.NotApplicable;
            }
            return GroupType.Unknown;
        }

        protected string MergeString(string thisProperty, string otherProperty)
        {
            if (thisProperty != otherProperty)
            {
                if (String.IsNullOrWhiteSpace(thisProperty))
                {
                    return otherProperty;
                }
                else if ((String.IsNullOrWhiteSpace(otherProperty) == false) &&
                         (thisProperty.IndexOf(otherProperty, StringComparison.Ordinal) == -1))
                {
                    return thisProperty + ", " + otherProperty;
                }
            }
            return thisProperty;
        }
    }
}
