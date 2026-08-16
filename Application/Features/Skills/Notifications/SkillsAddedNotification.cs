using MediatR;
using System.Collections.Generic;

namespace Application.Features.Skills.Notifications
{
    public class SkillsAddedNotification : INotification
    {
        public List<string> AddedSkillNames { get; }
        public string Source { get; }

        public SkillsAddedNotification(List<string> addedSkillNames, string source = "System")
        {
            AddedSkillNames = addedSkillNames ?? new List<string>();
            Source = source;
        }

        public SkillsAddedNotification(string singleSkillName, string source = "System")
        {
            AddedSkillNames = new List<string> { singleSkillName };
            Source = source;
        }
    }
}
