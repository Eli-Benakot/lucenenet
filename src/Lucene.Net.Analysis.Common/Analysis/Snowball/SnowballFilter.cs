﻿using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Tartarus.Snowball;
using System;
using System.Reflection;

namespace Lucene.Net.Analysis.Snowball
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
    /// A filter that stems words using a Snowball-generated stemmer.
    /// 
    /// Available stemmers are listed in <see cref="org.tartarus.snowball.ext"/>.
    /// <para><b>NOTE</b>: SnowballFilter expects lowercased text.
    /// <ul>
    ///  <li>For the Turkish language, see <see cref="TurkishLowerCaseFilter"/>.
    ///  <li>For other languages, see <see cref="LowerCaseFilter"/>.
    /// </ul>
    /// </para>
    /// 
    /// <para>
    /// Note: This filter is aware of the <see cref="KeywordAttribute"/>. To prevent
    /// certain terms from being passed to the stemmer
    /// <see cref="KeywordAttribute#isKeyword()"/> should be set to <code>true</code>
    /// in a previous <see cref="TokenStream"/>.
    /// 
    /// Note: For including the original term as well as the stemmed version, see
    /// <see cref="org.apache.lucene.analysis.miscellaneous.KeywordRepeatFilterFactory"/>
    /// </para>
    /// 
    /// 
    /// </summary>
    public sealed class SnowballFilter : TokenFilter
    {

        private readonly SnowballProgram stemmer;

        private readonly ICharTermAttribute termAtt;
        private readonly IKeywordAttribute keywordAttr;

        public SnowballFilter(TokenStream input, SnowballProgram stemmer)
              : base(input)
        {
            this.stemmer = stemmer;
            this.termAtt = AddAttribute<ICharTermAttribute>();
            this.keywordAttr = AddAttribute<IKeywordAttribute>();
        }

        /// <summary>
        /// Construct the named stemming filter.
        /// 
        /// Available stemmers are listed in <see cref="org.tartarus.snowball.ext"/>.
        /// The name of a stemmer is the part of the class name before "Stemmer",
        /// e.g., the stemmer in <see cref="org.tartarus.snowball.ext.EnglishStemmer"/> is named "English".
        /// </summary>
        /// <param name="in"> the input tokens to stem </param>
        /// <param name="name"> the name of a stemmer </param>
        public SnowballFilter(TokenStream @in, string name)
              : base(@in)
        {
            try
            {
                // LUCENENET TODO: There should probably be a way to make this an extesibility point so
                // custom extensions can be loaded.
                string className = typeof(SnowballProgram).Namespace + ".Ext." +
                    name + "Stemmer, " + this.GetType().GetTypeInfo().Assembly.GetName().Name;
                Type stemClass = Type.GetType(className);

                stemmer = (SnowballProgram)Activator.CreateInstance(stemClass);
            }
            catch (Exception e)
            {
                throw new System.ArgumentException("Invalid stemmer class specified: " + name, e);
            }

            this.termAtt = AddAttribute<ICharTermAttribute>();
            this.keywordAttr = AddAttribute<IKeywordAttribute>();
        }

        /// <summary>
        /// Returns the next input Token, after being stemmed </summary>
        public override bool IncrementToken()
        {
            if (m_input.IncrementToken())
            {
                if (!keywordAttr.IsKeyword)
                {
                    char[] termBuffer = termAtt.Buffer;
                    int length = termAtt.Length;
                    stemmer.SetCurrent(termBuffer, length);
                    stemmer.Stem();
                    char[] finalTerm = stemmer.CurrentBuffer;
                    int newLength = stemmer.CurrentBufferLength;
                    if (finalTerm != termBuffer)
                    {
                        termAtt.CopyBuffer(finalTerm, 0, newLength);
                    }
                    else
                    {
                        termAtt.Length = newLength;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}