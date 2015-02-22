using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using d60.Cirqus.Events;
using d60.Cirqus.Extensions;
using d60.Cirqus.Numbers;
using d60.Cirqus.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Exchanger
{
    public class EventSerializer : IDomainEventSerializer
    {
        readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver(),
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            TypeNameHandling = TypeNameHandling.Objects,
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
        };

        public EventData Serialize(DomainEvent e)
        {
            var serializedObject = JsonConvert.SerializeObject(e, jsonSerializerSettings);
            var eventData = EventData.FromDomainEvent(e, Encoding.UTF8.GetBytes(serializedObject));
            eventData.MarkAsJson();

            return eventData;
        }

        public DomainEvent Deserialize(EventData e)
        {
            var domainEvent = JsonConvert.DeserializeObject<DomainEvent>(
                Encoding.UTF8.GetString(e.Data),
                jsonSerializerSettings);

            Assign(domainEvent.Meta, e.Meta);

            return domainEvent;
        }

        public JsonSerializer CreateSerializer()
        {
            return JsonSerializer.Create(jsonSerializerSettings);
        }

        static void Assign(Metadata to, Metadata from)
        {
            foreach (var kvp in from)
            {
                to.Add(kvp.Key, kvp.Value);
            }
        }

        class DefaultContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
        {
            readonly static Regex matchesBackingFieldForAutoProperty = new Regex(@"\<(?<name>.*?)\>k__BackingField");

            public DefaultContractResolver() : base(shareCache: true) { }

            protected override JsonObjectContract CreateObjectContract(Type objectType)
            {
                var contract = base.CreateObjectContract(objectType);
                contract.DefaultCreator = () => FormatterServices.GetUninitializedObject(objectType);

                EnableInvisibleMetadata(contract);

                return contract;
            }

            protected override List<MemberInfo> GetSerializableMembers(Type objectType)
            {
                var members = new List<MemberInfo>();

                while (objectType != null)
                {
                    members.AddRange(
                        objectType.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                                  .OfType<FieldInfo>());

                    objectType = objectType.BaseType;
                }

                return members;
            }

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                return base.CreateProperties(type, memberSerialization).OrderBy(x => x.PropertyName).ToList();
            }

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                property.Writable = member.MemberType == MemberTypes.Field;
                property.Readable = member.MemberType == MemberTypes.Field;

                NormalizeAutoPropertyBackingFieldName(property);
                UppercaseFirstLetterOfFieldName(property);

                return property;
            }

            static void NormalizeAutoPropertyBackingFieldName(JsonProperty property)
            {
                var match = matchesBackingFieldForAutoProperty.Match(property.PropertyName);
                property.PropertyName = match.Success ? match.Groups["name"].Value : property.PropertyName;
            }

            static void UppercaseFirstLetterOfFieldName(JsonProperty property)
            {
                property.PropertyName =
                    property.PropertyName.First().ToString(CultureInfo.InvariantCulture).ToUpper() +
                    property.PropertyName.Substring(1);
            }

            static void EnableInvisibleMetadata(JsonObjectContract contract)
            {
                if (!typeof(DomainEvent).IsAssignableFrom(contract.UnderlyingType))
                    return;

                var property = contract.Properties["Meta"];

                property.Ignored = true;
                contract.OnDeserializedCallbacks.Add((target, context) =>
                                                         property.ValueProvider.SetValue(target, new Metadata()));
            }
        }
    }
}