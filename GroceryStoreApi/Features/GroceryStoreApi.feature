Feature: GroceryStoreApi

Test Grocery Store API

@getApiStatus

Scenario: Get API status
	Given I set a GET request at the API endpoint /status 
	When I send the API request
	Then the API should return the status 200


@createNewCart

Scenario: Create new cart
	Given I set a POST request at the API endpoint /carts  
	When I send the API request
	Then the API should return the status 201
	And the API response object should have the following properties:
    | Property   | Expected Value | Store Property |
    | created    | True           |				   |
    | cartId     |                | cartId         |


@addItemToCart

Scenario: Add item to cart
    Given I have created a cart 
    And I set a POST request at the API path variable endpoint /carts/<:cartId>/items
    When I input the following values in the request body:
        | Property  | Value |
		| productId | 4646  |
        | quantity  | 1     |
	And I send the API request
	Then the API should return the status 201
	And the API response object should have the following properties:
    | Property   | Expected Value | Store Property |
    | created    | True           |				   |
    | itemId     |                | itemId         |
    And the cart should now contain the following items:
    | Product ID | Quantity |
    | 4646       | 1        |


  @addMultipleItemsToCart

  Scenario: Add multiple items to cart
    Given I have created a cart
    And I set a POST request at the API path variable endpoint /carts/<:cartId>/items
    When I add the following items to the cart:
        | Product ID | Quantity |
        | 4646       | 1        |
        | 4643       | 2        |
    Then the cart should now contain the following items:
        | Product ID | Quantity |
        | 4646       | 1        |
        | 4643       | 2        |

  @getItemsFromCart

Scenario: Get items from cart
    Given I have created a cart and added an item
    And I set a GET request at the API path variable endpoint /carts/<:cartId>/items
    When I send the API request
    Then the API should return the status 200
	And the API response object should have the following properties:
    | Property | Expected Value | Store Property |
    | id       | <itemId>       |				 |
    | productId|    4646        |   productId    |
    | quantity |    1           |                |


@updateItemQuantity

Scenario: Update item quantity
    Given I have created a cart and added an item
    And I set a PATCH request at the API path variable endpoint /carts/<:cartId>/items/<:itemId>
    When I input the following values in the request body:
        | Property | Value  |
		| quantity | 2		|
	And I send the API request
	Then the API should return the status 204
    And the cart should now have the following properties:
        | Property  | Expected Value | Store Property |
        | id        |  <itemId>      |                |
        | quantity  |   2            |                |


@replaceItemInCart

Scenario: Replace item in cart
    Given I have created a cart and added an item
    And I set a PUT request at the API path variable endpoint /carts/<:cartId>/items/<:itemId>
    When I input the following values in the request body:
        | Property  | Value  |
        | productId | 4643	 |
		| quantity  | 2		 |
	And I send the API request
	Then the API should return the status 204
    And the cart should now have the following properties:
    | Property  | Expected Value | Store Property |
    | id        |  <itemId>      |                |
    | productId |  4643          |                |
    | quantity  |   2            |                |


@DeleteItemFromCart

Scenario: Delete item from cart
    Given I have created a cart and added an item 
    And I set a DELETE request at the API path variable endpoint /carts/<:cartId>/items/<:itemId>
	When I send the API request
	Then the API should return the status 204
    And the cart should now have the following properties:
    | Property  | Expected Value | Store Property |
    | id        | null           |                |
    | productId | null           |                |
    | quantity  | null           |                |


@createNewOrder

Scenario: Create a new order
	Given I get the API access token
	And I have created a cart and added an item
	And I set a POST request at the API endpoint /orders  
    When I input the following values in the request body:
        | Property      | Value         |
		| cartId        | <cartId>		|
		| customerName  | Mr.Bob		|
    When I send the API request
    Then the API should return the status 201
	And the API response object should have the following properties:
    | Property  | Expected Value | Store Property |
    | created   |    True          |                |
    | orderId   |                  |  orderId       |


@getSingleOrders

Scenario: Get single order
	Given I get the API access token
    And I have created a new order
    And I set a GET request at the API path variable endpoint /orders/<:orderId> 
	When I send the API request
	Then the API should return the status 200
	And the API response object should have the following properties:
    | Property   | Expected Value | Store Property |
    | created    |                |	created        |
    | id         |   <orderId>    |                |

@getAllOrders

Scenario: Get all orders
	Given I get the API access token
    And I have created a new order
	And I set a GET request at the API endpoint /orders  
	When I send the API request
	Then the API should return the status 200
	And the API response object should have the following properties:
    | Property   | Expected Value | Store Property |
    | created    |                |	created        |
    | id         |   <orderId>    |                |

