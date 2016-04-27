// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace StyleCop.Analyzers
{
    /// <summary>
    /// Provide list of analyzers
    /// </summary>
    public static class AnalyzerProvider
    {
        /// <summary>
        /// Discover all analyzers using reflection
        /// </summary>
        /// <returns>Enumerable of all analyzer objects</returns>
        public static IEnumerable<DiagnosticAnalyzer> GetAnalyzers()
        {
            var assembly = typeof(AnalyzerProvider).GetTypeInfo().Assembly;
            var analyzers = assembly.DefinedTypes.Where(t => t.BaseType == typeof(DiagnosticAnalyzer))
                .Where(y => !y.IsAbstract)
                .Select(x => (DiagnosticAnalyzer) Activator.CreateInstance(x.AsType()));
            return analyzers.AsEnumerable();
        }
    }
}
