using System.Collections.Generic;
using System.Linq;
using UbiqSecurity.Internals.Structured.Pipeline.Operations;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity.Internals.Structured.Pipeline
{
    internal class DecryptionPipeline : StructuredPipeline
    {
        private static readonly IEnumerable<IOperation> BaseOperations = new List<IOperation>()
        {
            new DecodeKeyNumberOperation(),
            new ConvertRadixOperation(),
            new DecryptOperation(),
        };

        public DecryptionPipeline(FfsRecord dataset)
            : base()
        {
            var operations = new List<IOperation>(BaseOperations);

            foreach (var rule in dataset.PassthroughRules.OrderByDescending(x => x.Priority))
            {
                switch (rule.RuleType)
                {
                    case PassthroughRuleType.Passthrough:
                        operations.Insert(0, new TrimPassthroughCharactersOperation());
                        operations.Add(new ExpandPassthroughCharactersOperation());
                        break;
                    case PassthroughRuleType.Prefix:
                        operations.Insert(0, new TrimPassthroughPrefixOperation());
                        operations.Add(new ExpandPassthroughPrefixOperation());
                        break;
                    case PassthroughRuleType.Suffix:
                        operations.Insert(0, new TrimPassthroughSuffixOperation());
                        operations.Add(new ExpandPassthroughSuffixOperation());
                        break;
                }
            }

            if (!dataset.PassthroughRules.Any() && !string.IsNullOrEmpty(dataset.PassthroughCharacters))
            {
                operations.Insert(0, new TrimPassthroughCharactersOperation());
                operations.Add(new ExpandPassthroughCharactersOperation());
            }

            Operations = operations;
        }
    }
}
