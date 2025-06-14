﻿// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using CommandLine.Infrastructure;
using CSharpx;

namespace CommandLine.Core
{
    enum SpecificationType
    {
        Option,
        Value
    }

    enum TargetType
    {
        Switch,
        Scalar,
        Sequence
    }

    abstract class Specification
    {
        private readonly SpecificationType tag;
        private readonly bool required;
        private readonly bool hidden;
        private readonly Maybe<int> min;
        private readonly Maybe<int> max;
        private readonly Maybe<object> defaultValue;
        private readonly string helpText;
        private readonly string metaValue;
        private readonly IEnumerable<string> enumValues;

        /// This information is denormalized to decouple Specification from PropertyInfo.
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
        private readonly Type conversionType;

        private readonly TargetType targetType;

        protected Specification(
            SpecificationType tag,
            bool required,
            Maybe<int> min,
            Maybe<int> max,
            Maybe<object> defaultValue,
            string helpText,
            string metaValue,
            IEnumerable<string> enumValues,
#if NET8_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
            Type conversionType,
            TargetType targetType,
            bool hidden = false)
        {
            this.tag = tag;
            this.required = required;
            this.min = min;
            this.max = max;
            this.defaultValue = defaultValue;
            this.conversionType = conversionType;
            this.targetType = targetType;
            this.helpText = helpText;
            this.metaValue = metaValue;
            this.enumValues = enumValues;
            this.hidden = hidden;
        }

        public SpecificationType Tag
        {
            get { return tag; }
        }

        public bool Required
        {
            get { return required; }
        }

        public Maybe<int> Min
        {
            get { return min; }
        }

        public Maybe<int> Max
        {
            get { return max; }
        }

        public Maybe<object> DefaultValue
        {
            get { return defaultValue; }
        }

        public string HelpText
        {
            get { return helpText; }
        }

        public string MetaValue
        {
            get { return metaValue; }
        }

        public IEnumerable<string> EnumValues
        {
            get { return enumValues; }
        }

#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
        public Type ConversionType
        {
            get { return conversionType; }
        }

        public TargetType TargetType
        {
            get { return targetType; }
        }

        public bool Hidden
        {
            get { return hidden; }
        }

#if NET8_0_OR_GREATER
        [UnconditionalSuppressMessage("Reflection", "IL2072")]
#endif
        public static Specification FromProperty(PropertyInfo property)
        {
            var attrs = property.GetCustomAttributes(true);
            var oa = attrs.OfType<OptionAttribute>().ToArray();

            var conversionType = property.PropertyType;
            if (oa.Length == 1)
            {
                var spec = OptionSpecification.FromAttribute(oa.Single(), conversionType,
                    ReflectionHelper.GetNamesOfEnum(conversionType));

                if (spec.ShortName.Length == 0 && spec.LongName.Length == 0)
                {
                    return spec.WithLongName(property.Name.ToLowerInvariant());
                }

                return spec;
            }

            var va = attrs.OfType<ValueAttribute>().ToArray();
            if (va.Length == 1)
            {
                return ValueSpecification.FromAttribute(va.Single(), conversionType,
                    conversionType.GetTypeInfo().IsEnum
                        ? Enum.GetNames(conversionType)
                        : Enumerable.Empty<string>());
            }

            throw new InvalidOperationException();
        }
    }
}
