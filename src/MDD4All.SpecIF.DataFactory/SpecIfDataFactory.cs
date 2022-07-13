/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataModels.Helpers;
using MDD4All.SpecIF.DataProvider.Contracts;
using System;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataFactory
{
    public class SpecIfDataFactory
    {
        private SpecIfDataFactory()
        {
        }

        public static Resource CreateResource(Key resourceClassKey, ISpecIfMetadataReader metadataReader)
        {
            Resource result = new Resource();

            ResourceClass resourceType = metadataReader.GetResourceClassByKey(resourceClassKey);

            result.ID = SpecIfGuidGenerator.CreateNewSpecIfGUID();
            result.Revision = SpecIfGuidGenerator.CreateNewRevsionGUID();
            result.Properties = new List<Property>();

            result.Class = resourceClassKey;

            foreach (Key propertyClassReference in resourceType.PropertyClasses)
            {
                Property property = new Property()
                {

                    Class = propertyClassReference,
                    Values = new List<Value>()
                };

                result.Properties.Add(property);
            }

            AddInheritedPropertiesRecursively(result, resourceType, metadataReader);

            result.ChangedAt = DateTime.Now;
            
            result.ChangedBy = Environment.UserName;

            return result;
        }

        public static Statement CreateStatement(Key statementClassKey, 
                                                Key subjectKey,
                                                Key objectKey,
                                                ISpecIfMetadataReader metadataReader)
        {
            Statement result = new Statement();

            StatementClass statementClass = metadataReader.GetStatementClassByKey(statementClassKey);

            result.ID = SpecIfGuidGenerator.CreateNewSpecIfGUID();
            result.Revision = SpecIfGuidGenerator.CreateNewRevsionGUID();
            result.Properties = new List<Property>();

            result.Class = statementClassKey;

            if (statementClass.PropertyClasses != null)
            {
                foreach (Key propertyClassReference in statementClass.PropertyClasses)
                {
                    Property property = new Property()
                    {

                        Class = propertyClassReference,
                        Values = new List<Value>()
                    };

                    result.Properties.Add(property);
                }
            }
            AddInheritedPropertiesRecursively(result, statementClass, metadataReader);

            result.ChangedAt = DateTime.Now;

            result.ChangedBy = Environment.UserName;

            result.StatementSubject = subjectKey;
            result.StatementObject = objectKey;


            return result;
        }

        public static Resource CreateResourceWithNode(Key resourceKey, out Node node, ISpecIfMetadataReader metadataReader)
        {
            Resource resource = CreateResource(resourceKey, metadataReader);

            node = new Node
            {
                ResourceReference = new Key(resource.ID, resource.Revision)
            };

            return resource;
        }


        private static void AddInheritedPropertiesRecursively(Resource result, ResourceClass currentResourceClass, ISpecIfMetadataReader metadataReader)
        {
            if(currentResourceClass.Extends != null)
            {
                Key parentClassKey = currentResourceClass.Extends;

                ResourceClass parentResourceClass = metadataReader.GetResourceClassByKey(parentClassKey);

                if (parentResourceClass != null)
                {
                    foreach (Key propertyClassReference in parentResourceClass.PropertyClasses)
                    {
                        PropertyClass propertyClass = metadataReader.GetPropertyClassByKey(propertyClassReference);

                        if (!IsPropertyStillIncluded(result.Properties, propertyClass.Title, metadataReader))
                        {
                            Property property = new Property()
                            {

                                Class = propertyClassReference,
                                Values = new List<Value>()
                            };

                            result.Properties.Add(property);
                        }
                    }
                    AddInheritedPropertiesRecursively(result, parentResourceClass, metadataReader);
                }

                

            }

            
        }

        private static bool IsPropertyStillIncluded(List<Property> propertyList, string propertyClassTitle, ISpecIfMetadataReader metadataReader)
        {
            bool result = false;

            foreach(Property property in propertyList)
            {
                PropertyClass propertyClass = metadataReader.GetPropertyClassByKey(property.Class);

                if(propertyClass.Title == propertyClassTitle)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }
    }
}
