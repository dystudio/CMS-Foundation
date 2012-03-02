﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Xml.Linq;
using System.Linq;
using Composite.Data;
using Composite.Functions.Foundation;
using Composite.Core.Parallelization;
using Composite.Core.Types;
using Composite.Core.Xml;


namespace Composite.Functions
{
	internal sealed class XElementParameterRuntimeTreeNode : BaseParameterRuntimeTreeNode
    {
        private XElement _element;

        private XElement ExecuteInnerFunctions(FunctionContextContainer contextContainer)
        {
            XElement resultRoot = new XElement(_element);
            int loopCount = 0;

            while (true)
            {
                XName functionXName = Namespaces.Function10 + FunctionTreeConfigurationNames.FunctionTagName;
                IEnumerable<XElement> nestedFunctionCalls = resultRoot.Descendants(functionXName).Where(f => !f.Ancestors(functionXName).Any());
                var evaluatedListOfInnerFunctions = nestedFunctionCalls.ToList();

                if (!evaluatedListOfInnerFunctions.Any())
                {
                    break;
                }

                if (loopCount++ > 1000)
                {
                    throw new InvalidOperationException("One or more function seems to be returning markup generating endless recursion. The following markup seems to generate the problem: " + evaluatedListOfInnerFunctions.First().ToString());
                }

                var functionCallResults = new object[evaluatedListOfInnerFunctions.Count];

                ParallelFacade.For("Functions. Executing nested function calls",
                    0, evaluatedListOfInnerFunctions.Count, i =>
                    {
                        XElement functionCallDefinition = evaluatedListOfInnerFunctions[i];

                        BaseRuntimeTreeNode runtimeTreeNode = FunctionTreeBuilder.Build(functionCallDefinition);

                        functionCallResults[i] = runtimeTreeNode.GetValue(contextContainer);
                    });

                for (int i = 0; i < evaluatedListOfInnerFunctions.Count; i++)
                {
                    object embedableResult = contextContainer.MakeXEmbedable(functionCallResults[i]);

                    if (embedableResult != null && embedableResult is XAttribute)
                    {
                        evaluatedListOfInnerFunctions[i].Parent.Add(embedableResult);
                        evaluatedListOfInnerFunctions[i].Remove();
                    }
                    else
                    {
                        evaluatedListOfInnerFunctions[i].ReplaceWith(embedableResult);
                    }
                }
            }

            return resultRoot;
        }


        public XElementParameterRuntimeTreeNode(string name, XElement element)
            : base(name)
        {
            _element = element;
        }


        public override object GetValue(FunctionContextContainer contextContainer)
        {
            if (contextContainer == null) throw new ArgumentNullException("contextContainer");

            return ExecuteInnerFunctions(contextContainer);
        }


        public override object GetCachedValue(FunctionContextContainer contextContainer)
        {
            if (contextContainer == null) throw new ArgumentNullException("contextContainer");

            return ExecuteInnerFunctions(contextContainer);
        }


        public override IEnumerable<string> GetAllSubFunctionNames()
        {
            return
                from nameAttribute in _element.Elements(Namespaces.Function10 + FunctionTreeConfigurationNames.FunctionTagName).Attributes( FunctionTreeConfigurationNames.NameAttributeName )
                select nameAttribute.Value;
        }



        public override bool ContainsNestedFunctions
        {
            get
            {
                return GetAllSubFunctionNames().Any();
            }
        }



        public XElement GetHostedXElement()
        {
            return _element;
        }

        public override XElement Serialize()
        {
            XElement element =
                new XElement(XName.Get(FunctionTreeConfigurationNames.ParamTagName, FunctionTreeConfigurationNames.NamespaceName),
                    new XAttribute(FunctionTreeConfigurationNames.NameAttributeName, this.Name),
                    _element
                );

            return element;
        }
    }
}
