using System.Collections.Generic;
using System;

public class Additional_info
{
    public object authentication_code { get; set; }
    public object available_balance { get; set; }
    public object nsu_processadora { get; set; }

}
public class Card
{

}
public class Accounts
{
    public string from { get; set; }
    public string to { get; set; }

}
public class Amounts
{
    public double original { get; set; }
    public Int64 refunded { get; set; }

}
public class Metadata
{

}
public class Charges_details
{
    public Accounts accounts { get; set; }
    public Amounts amounts { get; set; }
    public Int64 client_id { get; set; }
    public DateTime date_created { get; set; }
    public string id { get; set; }
    public DateTime last_updated { get; set; }
    public Metadata metadata { get; set; }
    public string name { get; set; }
    public object refund_charges { get; set; }
    public object reserve_id { get; set; }
    public string type { get; set; }

}
public class Fee_details
{
    public double amount { get; set; }
    public string fee_payer { get; set; }
    public string type { get; set; }

}
//public class Metadata
//{

//}
public class Order
{
    public string id { get; set; }
    public string type { get; set; }

}
public class Payer
{
    public string id { get; set; }

}
public class Payment_method
{
    public string id { get; set; }
    public string issuer_id { get; set; }
    public string type { get; set; }

}
public class Business_info
{
    public string branch { get; set; }
    public string sub_unit { get; set; }
    public string unit { get; set; }

}
public class Location
{
    public string source { get; set; }
    public string state_id { get; set; }

}
public class Transaction_data
{
    public object e2e_id { get; set; }

}
public class Point_of_interaction
{
    public Business_info business_info { get; set; }
    public Location location { get; set; }
    public Transaction_data transaction_data { get; set; }
    public string type { get; set; }

}

public class Transaction_details
{
    public object acquirer_reference { get; set; }
    public object external_resource_url { get; set; }
    public object financial_institution { get; set; }
    public Int64 installment_amount { get; set; }
    public double net_received_amount { get; set; }
    public Int64 overpaid_amount { get; set; }
    public object payable_deferral_period { get; set; }
    public object payment_method_reference_id { get; set; }
    public double total_paid_amount { get; set; }

}
public class PaymentoResponse
{
    public object accounts_info { get; set; }
    public object acquirer_reconciliation { get; set; }
    public Additional_info additional_info { get; set; }
    public object authorization_code { get; set; }
    public bool binary_mode { get; set; }
    public object brand_id { get; set; }
    public string build_version { get; set; }
    public object call_for_authorize_id { get; set; }
    public bool captured { get; set; }
    public Card card { get; set; }
    public IList<Charges_details> charges_details { get; set; }
    public Int64 collector_id { get; set; }
    public object corporation_id { get; set; }
    public object counter_currency { get; set; }
    public Int64 coupon_amount { get; set; }
    public string currency_id { get; set; }
    public DateTime date_approved { get; set; }
    public DateTime date_created { get; set; }
    public DateTime date_last_updated { get; set; }
    public object date_of_expiration { get; set; }
    public object deduction_schema { get; set; }
    public string description { get; set; }
    public object differential_pricing_id { get; set; }

    public object ExternalReference { get; set; }
    public IList<Fee_details> fee_details { get; set; }
    public object financing_group { get; set; }
    public Int64 id { get; set; }
    public Int64 installments { get; set; }
    public object integrator_id { get; set; }
    public string issuer_id { get; set; }
    public bool live_mode { get; set; }
    public object marketplace_owner { get; set; }
    public object merchant_account_id { get; set; }
    public object merchant_number { get; set; }
    public Metadata metadata { get; set; }
    public DateTime money_release_date { get; set; }
    public object money_release_schema { get; set; }
    public string money_release_status { get; set; }
    public object notification_url { get; set; }
    public string operation_type { get; set; }
    public Order order { get; set; }
    public Payer payer { get; set; }
    public Payment_method payment_method { get; set; }
    public string payment_method_id { get; set; }
    public string payment_type_id { get; set; }
    public object platform_id { get; set; }
    public Point_of_interaction point_of_interaction { get; set; }
    public string pos_id { get; set; }
    public string processing_mode { get; set; }
    public object refunds { get; set; }
    public Int64 shipping_amount { get; set; }
    public object sponsor_id { get; set; }
    public object statement_descriptor { get; set; }
    public string status { get; set; }
    public string status_detail { get; set; }
    public string store_id { get; set; }
    public object tags { get; set; }
    public Int64 taxes_amount { get; set; }
    public Int64 transaction_amount { get; set; }
    public Int64 transaction_amount_refunded { get; set; }
    public Transaction_details transaction_details { get; set; }

}