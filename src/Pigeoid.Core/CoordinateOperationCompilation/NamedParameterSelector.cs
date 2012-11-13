using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Pigeoid.Contracts;
using JetBrains.Annotations;

namespace Pigeoid.CoordinateOperationCompilation
{
	public abstract class NamedParameterSelector
	{

		public class ParameterData
		{

			public ParameterData([NotNull] INamedParameter namedParameter, [NotNull] string normalizedName) {
				if(null == namedParameter)
					throw new ArgumentNullException("namedParameter");
				if(null == normalizedName)
					throw new ArgumentNullException("normalizedName");

				NamedParameter = namedParameter;
				NormalizedName = normalizedName;
			}

			public INamedParameter NamedParameter { get; private set; }
			public string NormalizedName { get; private set; }
		}

		public static bool AllAreSelected([NotNull] params NamedParameterSelector[] selectors){
			if (selectors.Length == 0)
				return false;
			for (int i = 0; i < selectors.Length; i++ ){
				if (!selectors[i].IsSelected)
					return false;
			}
			return true;
		}

		protected NamedParameterSelector(){
			Selection = null;
		}

		public bool IsSelected { get { return null != Selection; } }
		public INamedParameter Selection { get; private set; }

		public abstract int Score(ParameterData parameterData);

		public bool Select(INamedParameter namedParameter){
			if (null != Selection)
				return false;

			Selection = namedParameter;
			return true;
		}
	}

	public class KeywordNamedParameterSelector : NamedParameterSelector
	{


		private readonly string[] _keywords;

		public KeywordNamedParameterSelector([NotNull, InstantHandle] IEnumerable<string> keywords)
			: this(keywords.ToArray()) { }

		public KeywordNamedParameterSelector([NotNull] params string[] keywords){
			_keywords = keywords;
		}

		public ReadOnlyCollection<string> Keywords { get { return Array.AsReadOnly(_keywords); } }

		public override int Score(ParameterData parameterData){
			var parameterName = parameterData.NormalizedName;
			if(String.IsNullOrEmpty(parameterName))
				return 0;

			var score = 0;
			for (int keywordIndex = 0; keywordIndex < _keywords.Length; keywordIndex++){
				if (parameterName.Contains(_keywords[keywordIndex]))
					score++;
			}
			return score;
		}
	}

}
