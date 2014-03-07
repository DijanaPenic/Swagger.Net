using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Swagger.Net.WebApi.Controllers
{
    public class TagsController : ApiController
    {
        /// <summary>
        /// Get all of the Tags
        /// </summary>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Get a single Tag by it's id
        /// </summary>
        public string Get(int id)
        {
            return "value";
        }

        /// <summary>
        /// Create a new Tag
        /// </summary>
        /// <param name="value"></param>
        public void Post([FromBody]string value)
        {
        }

        /// <summary>
        /// This is example description for Put method
        /// </summary>
        /// <param name="id"> This is example description for id parameter</param>
        /// <param name="value"> This is example description for value parameter </param>
        /// <example> {"totalRecords":1,"recordsPerPage":50,"page":1,"searchQuery":"","sort":"","embed":"","links":{"find":{"href":"http://server/","templated":true},"next":{"href":"http://server/","templated":false}},"embedded":{"item":[{"key":"key","value":"value","id":"65eceb29-484d-4b58-a27a-a261013707ee","dateCreated":"2013-10-24T14:59:11.18","dateUpdated":"2013-10-24T14:59:11.18","links":{"self":{"href":"http://server/","templated":false},"post":{"href":"http://server/","templated":false},"put":{"href":"http://server/","templated":false},"delete":{"href":"http://server/","templated":false}},"embedded":{}}]}}  </example>
        /// <code>
        /// public virtual string GetNotes(HttpActionDescriptor actionDescriptor)
        /// {
        ///    XPathNavigator memberNode = GetMemberNode(actionDescriptor);
        ///    if (memberNode != null)
        ///    {
        ///        XPathNavigator summaryNode = memberNode.SelectSingleNode("remarks");
        ///        if (summaryNode != null)
        ///        {
        ///            // This is comment example
        ///            var NewLine = 15
        ///            var SpaceReplace = 10;
        ///            return SpaceReplace;
        ///        }
        ///    }
        ///    return "No Documentation Found.";
        /// }
        /// </code>
        /// <remarks>  
        /// For more details please check: http://www.google.com
        /// 
        ///     Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's  standard dummy text ever since the 1500s, when an unknown standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries,but also the leap into electronic typesetting,
        /// 
        ///     Remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.    
        /// </remarks>
        /// <returns> This function returns status for Put Web API call (For more details please check: http://www.google.com) </returns>
        /// <responseCodes> 
        ///     <response>
        ///         <code>500</code>
        ///         <message>Internal server errors</message>
        ///     </response>
        ///     <response>
        ///         <code>200</code>
        ///         <message>Unknown server errors</message>
        ///     </response>
        /// </responseCodes>
        public void Put(int id, [FromBody]string value)
        {
        }

        /// <summary>
        /// Remove a Tag by it's id
        /// </summary>
        /// <param name="id"></param>
        public void Delete(int id)
        {
        }
    }
}
