using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Tdmts.Magento1.ServiceReferenceMagento;

namespace Tdmts.Magento1
{
    public class MagentoSOAPClient
    {
        /**
        * Basic store properties
        */
        private string _endpointUrl;
        private string _store;
        private string _username;
        private string _password;
        private string _accessToken;

        /**
        * Clients 
        */
        private Mage_Api_Model_Server_V2_HandlerPortTypeClient _serviceMagentoV1;

        public MagentoSOAPClient(string endpointUrl, string store, string accessToken, string username, string password)
        {
            EndpointUrl = endpointUrl;
            Store = store;
            Username = username;
            Password = password;
        }

        #region ServiceInitializers

        private Mage_Api_Model_Server_V2_HandlerPortTypeClient ServiceMagentoV1()
        {
            if (_serviceMagentoV1 == null && _serviceMagentoV1.State != CommunicationState.Opened)
            {
                var client = new Mage_Api_Model_Server_V2_HandlerPortTypeClient(BasicHttpBinding, new EndpointAddress(EndpointUrl));
                _serviceMagentoV1 = client;
            }
            return _serviceMagentoV1;
        }

        #endregion

        #region Connection to Magento Service

        public async Task<string> ConnectAsync()
        {
            try
            {
                Mage_Api_Model_Server_V2_HandlerPortTypeClient service = ServiceMagentoV1();

                AccessToken = await service.loginAsync(Username, Password);

                return AccessToken;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool Disconnect()
        {
            try
            {
                EndpointUrl = string.Empty;
                Store = string.Empty;
                AccessToken = string.Empty;
                _serviceMagentoV1.Close();
                _serviceMagentoV1 = null;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        private async Task<List<catalogProductReturnEntity>> GetProductByRequest(catalogProductListRequest request)
        {
            try
            {
                List<catalogProductReturnEntity> result = new List<catalogProductReturnEntity>();

                catalogProductListResponse response = await _serviceMagentoV1.catalogProductListAsync(request);

                foreach (var product in response.storeView)
                {

                    catalogAttributeEntity[] attributeListAttributes = await _serviceMagentoV1.catalogProductAttributeListAsync(AccessToken, int.Parse(product.set));

                    List<string> productAttributes = new List<string>();
                    foreach (var attributeListAttribute in attributeListAttributes)
                    {
                        productAttributes.Add(attributeListAttribute.code);
                    }

                    catalogProductRequestAttributes requestAttributes = new catalogProductRequestAttributes();
                    requestAttributes.attributes = productAttributes.ToArray();
                    requestAttributes.additional_attributes = productAttributes.ToArray();

                    catalogProductReturnEntity productReturnEntity = await _serviceMagentoV1.catalogProductInfoAsync(AccessToken, product.product_id, "0", requestAttributes, "id");

                    result.Add(productReturnEntity);
                }

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<catalogProductReturnEntity>> GetProductsAll()
        {
            try
            {
                List<catalogProductReturnEntity> result = new List<catalogProductReturnEntity>();

                catalogProductListRequest request = new catalogProductListRequest();
                filters filters = new filters();
                request.filters = filters;
                request.sessionId = AccessToken;
                request.storeView = "0";

                result = await GetProductByRequest(request);

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<catalogProductReturnEntity>> GetProductsBySKU(string storeView, string sku)
        {
            try
            {
                List<catalogProductReturnEntity> result = new List<catalogProductReturnEntity>();

                catalogProductListRequest request = new catalogProductListRequest();
                filters filters = new filters();
                List<associativeEntity> aes = new List<associativeEntity>();
                associativeEntity ae = new associativeEntity();
                ae.key = "sku";
                ae.value = sku;
                aes.Add(ae);
                filters.filter = aes.ToArray();
                request.filters = filters;
                request.sessionId = AccessToken;
                request.storeView = storeView;
                result = await GetProductByRequest(request);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<catalogProductAttributeSetEntity>> GetAttributeSetByName(string name)
        {
            try
            {
                List<catalogProductAttributeSetEntity> result = new List<catalogProductAttributeSetEntity>();

                catalogProductAttributeSetEntity[] cpases = await _serviceMagentoV1.catalogProductAttributeSetListAsync(AccessToken);
                foreach (catalogProductAttributeSetEntity cpase in cpases)
                {
                    if (cpase.name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Add(cpase);
                    }
                }

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<catalogProductAttributeSetEntity>> GetAttributeSetById(int id)
        {
            try
            {
                List<catalogProductAttributeSetEntity> result = new List<catalogProductAttributeSetEntity>();

                catalogProductAttributeSetEntity[] cpases = await _serviceMagentoV1.catalogProductAttributeSetListAsync(AccessToken);
                foreach (catalogProductAttributeSetEntity cpase in cpases)
                {
                    if (cpase.set_id == id)
                    {
                        result.Add(cpase);
                    }
                }

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private BasicHttpBinding BasicHttpBinding
        {
            get
            {
                BasicHttpBinding basicHttpBinding = new BasicHttpBinding();
                basicHttpBinding.MaxBufferSize = 2147483647;
                basicHttpBinding.MaxReceivedMessageSize = 2147483647;
                basicHttpBinding.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;
                basicHttpBinding.ReceiveTimeout = new TimeSpan(0, 5, 0);
                return basicHttpBinding;
            }
        }

        public string EndpointUrl
        {
            get
            {
                return _endpointUrl;
            }

            set
            {
                _endpointUrl = value;
            }
        }

        public string Store
        {
            get
            {
                return _store;
            }

            set
            {
                _store = value;
            }
        }

        public string Username
        {
            get
            {
                return _username;
            }

            set
            {
                _username = value;
            }
        }

        public string Password
        {
            get
            {
                return _password;
            }

            set
            {
                _password = value;
            }
        }

        public string AccessToken
        {
            get
            {
                return _accessToken;
            }

            set
            {
                _accessToken = value;
            }
        }
    }
}
