﻿using Icu.Collation;
using Lucene.Net.Collation.TokenAttributes;
using Lucene.Net.Util;
using System.Reflection;

namespace Lucene.Net.Collation
{
    /*
     * Licensed to the Apache Software Foundation (ASF) under one or more
     * contributor license agreements.  See the NOTICE file distributed with
     * this work for additional information regarding copyright ownership.
     * The ASF licenses this file to You under the Apache License, Version 2.0
     * (the "License"); you may not use this file except in compliance with
     * the License.  You may obtain a copy of the License at
     *
     *     http://www.apache.org/licenses/LICENSE-2.0
     *
     * Unless required by applicable law or agreed to in writing, software
     * distributed under the License is distributed on an "AS IS" BASIS,
     * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
     * See the License for the specific language governing permissions and
     * limitations under the License.
     */

    /// <summary>
    /// <para>
    ///   Converts each token into its <see cref="CollationKey"/>, and then
    ///   encodes the bytes as an index term.
    /// </para>
    /// <para>
    ///   <strong>WARNING:</strong> Make sure you use exactly the same Collator at
    ///   index and query time -- CollationKeys are only comparable when produced by
    ///   the same Collator.  Since <see cref="RuleBasedCollator"/>s are not
    ///   independently versioned, it is unsafe to search against stored
    ///   CollationKeys unless the following are exactly the same (best practice is
    ///   to store this information with the index and check that they remain the
    ///   same at query time):
    /// </para>
    /// <ol>
    ///   <li>JVM vendor</li>
    ///   <li>JVM version, including patch version</li>
    ///   <li>
    ///     The language (and country and variant, if specified) of the Locale
    ///     used when constructing the collator via
    ///     <see cref="Collator#getInstance(Locale)"/>.
    ///   </li>
    ///   <li>
    ///     The collation strength used - see <see cref="Collator#setStrength(int)"/>
    ///   </li>
    /// </ol> 
    /// <para>
    ///   The <code>ICUCollationAttributeFactory</code> in the analysis-icu package 
    ///   uses ICU4J's Collator, which makes its
    ///   version available, thus allowing collation to be versioned independently
    ///   from the JVM.  ICUCollationAttributeFactory is also significantly faster and
    ///   generates significantly shorter keys than CollationAttributeFactory.  See
    ///   <a href="http://site.icu-project.org/charts/collation-icu4j-sun"
    ///   >http://site.icu-project.org/charts/collation-icu4j-sun</a> for key
    ///   generation timing and key length comparisons between ICU4J and
    ///   java.text.Collator over several languages.
    /// </para>
    /// <para>
    ///   CollationKeys generated by java.text.Collators are not compatible
    ///   with those those generated by ICU Collators.  Specifically, if you use 
    ///   CollationAttributeFactory to generate index terms, do not use
    ///   ICUCollationAttributeFactory on the query side, or vice versa.
    /// </para>
    /// </summary>
    // LUCENENET TODO: A better option would be to contribute to the icu.net library and
    // make it CLS compliant (at least the parts of it we use)
    [System.CLSCompliant(false)]
    public class CollationAttributeFactory : AttributeSource.AttributeFactory
	{
		private readonly Collator collator;
		private readonly AttributeSource.AttributeFactory @delegate;

		/// <summary>
		/// Create a CollationAttributeFactory, using 
		/// <see cref="AttributeSource.AttributeFactory#DEFAULT_ATTRIBUTE_FACTORY"/> as the
		/// factory for all other attributes. </summary>
		/// <param name="collator"> CollationKey generator </param>
		public CollationAttributeFactory(Collator collator) : this(AttributeSource.AttributeFactory.DEFAULT_ATTRIBUTE_FACTORY, collator)
		{
		}

		/// <summary>
		/// Create a CollationAttributeFactory, using the supplied Attribute Factory 
		/// as the factory for all other attributes. </summary>
		/// <param name="delegate"> Attribute Factory </param>
		/// <param name="collator"> CollationKey generator </param>
		public CollationAttributeFactory(AttributeSource.AttributeFactory @delegate, Collator collator)
		{
			this.@delegate = @delegate;
			this.collator = collator;
		}

		public override Attribute CreateAttributeInstance<T>()
		{
			return typeof(T).GetTypeInfo().IsAssignableFrom(typeof(CollatedTermAttributeImpl))
				? new CollatedTermAttributeImpl(this.collator)
				: this.@delegate.CreateAttributeInstance<T>();
		}
	}
}