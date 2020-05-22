using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ServiceModel.Description;

// Namespaces in Microsoft.Xrm.Sdk.dll
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;

// Namespace in Microsoft.Crm.Sdk.Proxy.dll
using Microsoft.Crm.Sdk.Messages;

namespace Metricon.WCF.MetriconRetailSystem.Services
{
    public static class CrmMetadataAccess
    {

        public static DataTable GetDropDownOptions(string entityName, string attributeName)
        {
            OrganizationServiceProxy serviceProxy = CrmHelper.Connect();

            DataTable options = new DataTable();
            options.Columns.Add("Value");
            options.Columns.Add("Name");   
       
            RetrieveAttributeRequest attributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = entityName,
                LogicalName = attributeName
            };

            RetrieveAttributeResponse attributeResponse = (RetrieveAttributeResponse)serviceProxy.Execute(attributeRequest);

            // Handle Picklist options 
            if (attributeResponse.AttributeMetadata.AttributeType == AttributeTypeCode.Picklist) 
            {
                PicklistAttributeMetadata picklist = (PicklistAttributeMetadata)attributeResponse.AttributeMetadata;
                foreach (OptionMetadata option in picklist.OptionSet.Options)
                {
                    DataRow optionDataRow = options.NewRow();
                    optionDataRow["Value"] = option.Value.Value;
                    optionDataRow["Name"] = option.Label.UserLocalizedLabel.Label;

                    options.Rows.Add(optionDataRow);
                }
            }

            // Handle Status options (Active State only)
            else if (attributeResponse.AttributeMetadata.AttributeType == AttributeTypeCode.Status)
            {
                StatusAttributeMetadata status = (StatusAttributeMetadata)attributeResponse.AttributeMetadata;
                foreach (StatusOptionMetadata option in status.OptionSet.Options)
                {
                    //if (option.State.Value == 0) // Active State
                    //{
                        DataRow optionDataRow = options.NewRow();
                        optionDataRow["Value"] = option.Value.Value;
                        optionDataRow["Name"] = option.Label.UserLocalizedLabel.Label;

                        options.Rows.Add(optionDataRow);
                    //}
                }
            }
            // Handle Status options (Active State only)
            else if (attributeResponse.AttributeMetadata.AttributeType == AttributeTypeCode.EntityName)
            {
                EntityNameAttributeMetadata name = (EntityNameAttributeMetadata)attributeResponse.AttributeMetadata;
                foreach (OptionMetadata option in name.OptionSet.Options)
                {
                    DataRow optionDataRow = options.NewRow();
                    optionDataRow["Value"] = option.Value.Value;
                    optionDataRow["Name"] = option.Label.UserLocalizedLabel.Label;

                    options.Rows.Add(optionDataRow);
                }
            }
            return options;
            
        }

        public static string GetDropDownValue(string entityName, string attributeName, int attributeValue)
        {
            string DropDownText = string.Empty;

            OrganizationServiceProxy serviceProxy = CrmHelper.Connect();

            RetrieveAttributeRequest attributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = entityName,
                LogicalName = attributeName
            };                 

            RetrieveAttributeResponse attributeResponse = (RetrieveAttributeResponse)serviceProxy.Execute(attributeRequest);
            
            // Handle Picklist options
            if (attributeResponse.AttributeMetadata.AttributeType == AttributeTypeCode.Picklist)
            {
                PicklistAttributeMetadata picklist = (PicklistAttributeMetadata)attributeResponse.AttributeMetadata;
                foreach (OptionMetadata option in picklist.OptionSet.Options)
                {
                    if (option.Value == attributeValue)
                    {
                        DropDownText = option.Label.UserLocalizedLabel.Label;
                        break;
                    }
                }
            }

            // Handle Status options
            else if (attributeResponse.AttributeMetadata.AttributeType == AttributeTypeCode.Status)
            {
                StatusAttributeMetadata status = (StatusAttributeMetadata)attributeResponse.AttributeMetadata;
                foreach (StatusOptionMetadata option in status.OptionSet.Options)
                {
                    if (option.Value == attributeValue)
                    {
                        DropDownText = option.Label.UserLocalizedLabel.Label;
                        break;
                    }
                }
            }
            return DropDownText;

        }

        public static AttributeMetadata[] GetEntityAttributes(string entityName)
        {
            OrganizationServiceProxy serviceProxy = CrmHelper.Connect();

            RetrieveEntityRequest entityRequest = new RetrieveEntityRequest
            {
                LogicalName = entityName,
                EntityFilters = EntityFilters.Attributes
            };

            RetrieveEntityResponse entityResponse = (RetrieveEntityResponse)serviceProxy.Execute(entityRequest);

            if (entityResponse != null)
                return entityResponse.EntityMetadata.Attributes;
            else
                return null;
        }

        public static string GetEntityDisplayName(string entitySchemaName)
        {
            OrganizationServiceProxy serviceProxy = CrmHelper.Connect();

            RetrieveEntityRequest entityRequest = new RetrieveEntityRequest
            {
                LogicalName = entitySchemaName,
                EntityFilters = EntityFilters.Entity
            };

            RetrieveEntityResponse entityResponse = (RetrieveEntityResponse)serviceProxy.Execute(entityRequest);

            if (entityResponse != null)
                return entityResponse.EntityMetadata.DisplayName.UserLocalizedLabel.Label;
            else
                return null;
        }
    }
}