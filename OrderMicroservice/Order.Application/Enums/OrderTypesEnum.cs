using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Enums
{
    public enum OrderTypesEnum
    {
     
            [Description("Cust.Independent Req")]
            I = 1,

            [Description("Promotion Order")]
            AA = 2,

            [Description("Qtn from Serv. Order")]
            AE = 3,

            [Description("Standard order")]
            AEBO = 4,

            [Description("Offer")]
            AEBQ = 5,

            [Description("Project Quotation")]
            AP = 6,

            [Description("Repair Quotation")]
            AR = 7,

            [Description("Service Quotation")]
            AS = 8,

            [Description("Quotation f.Contract")]
            AV = 9,

            [Description("Reb.Credit Memo Req")]
            B1 = 10,

            [Description("Exptd RebateCredMemo")]
            B1E = 11,

            [Description("Rebate Correctn Rqst")]
            B2 = 12,

            [Description("Rebate P.Correctn Rt")]
            B2E = 13,

            [Description("PartRebSettl.Request")]
            B3 = 14,

            [Description("Exp.PartRebSettl.Req")]
            B3E = 15,

            [Description("Reb.Req.f.Man.Accrls")]
            B4 = 16,

            [Description("Indir. Sales Rebate")]
            BIND = 17,

            [Description("Agrmt Cred.Memo Req")]
            BK1 = 18,

            [Description("Agrmt Cred.Memo Req")]
            BK3 = 19,

            [Description("Agrmnt Deb.Memo Req")]
            BM1 = 20,

            [Description("Agrmnt Deb.Memo Req")]
            BM3 = 21,

            [Description("Service Contract BDR")]
            BSC = 22,

            [Description("Service Confirm eBDR")]
            BSVC = 23,

            [Description("Service Order eBDR ")]
            BSVO = 24,

            [Description("Cash Sale")]
            BV = 25,

            [Description("CF Part.Rebate Sett.")]
            CFB3 = 26,

            [Description("CF Credit Memo Req")]
            CFG2 = 27,

            [Description("Contract Handling")]
            CH = 28,

            [Description("Requests + Returns")]
            CLRP = 29,

            [Description("Standard Order")]
            CMDM = 30,

            [Description("Standard Order")]
            CMR = 31,

            [Description("Standard Order")]
            CMRC = 32,

            [Description("Standard Order")]
            CMRP = 33,

            [Description("Quantity Contract")]
            CQ = 34,

            [Description("Credit Memo Request")]
            CR = 35,

            [Description("SlsDocTypeDelyHUmvmt")]
            DHU = 36,

            [Description("Order Type Sched.Ag.")]
            DL = 37,

            [Description("ARM Cust.Ret.Deliv.")]
            DL2 = 38,

            [Description("Ord.Type Returns Del")]
            DLR = 39,

            [Description("Ord.Type Returns Del")]
            DLRE = 40,

            [Description("Standard Order")]
            DMRB = 41,

            [Description("Standard Order")]
            DMRP = 42,

            [Description("Standard Order")]
            DMRR = 43,

            [Description("Debit Memo Request")]
            DR = 44,

            [Description("Dec. Dely Order Type")]
            DZL = 45,

            [Description("Issue by Ext. Agent")]
            ED = 46,

            [Description("Correction f.ExtAgnt")]
            EDKO = 47,

            [Description("Deliv.Free of Charge")]
            FD = 48,

            [Description("Credit Memo Req. Val")]
            G2WT = 49,

            [Description("Credit Memo Request")]
            GA2 = 50,


            [Description("GG Quantity Contract")]
            GCQ = 51,

            [Description("Standard Order")]
            GCTA = 52,

            [Description("Master Contract")]
            GK = 53,

            [Description("GG Standard Order")]
            GOR = 54,

            [Description("Inquiry")]
            HBIN = 55,

            [Description("Inquiry")]
            IBOS = 56,

            [Description("Customer Price List")]
            ICPL = 57,

            [Description("Inquiry")]
            IN = 58,

            [Description("Shp&Deb.Qty Contract")]
            JSDQ = 59,

            [Description("Consignment Pick-up")]
            KA = 60,

            [Description("ConsignPick-up CompS")]
            KAZU = 61,

            [Description("Consignment Fill-up")]
            KB = 62,

            [Description("Consignment Issue")]
            KE = 63,

            [Description("Consignment Returns")]
            KR = 64,

            [Description("Expense:DebitMemoReq")]
            L2DM = 65,

            [Description("Expense:Payment Req.")]
            L2DP = 66,

            [Description("Debit Memo Req Value")]
            L2WT = 67,

            [Description("Ret.Packaging Pickup")]
            LA = 68,

            [Description("Sched.Agreement ExAg")]
            LK = 69,

            [Description("JIT Sched Agr w/cne")]
            LKJ = 70,

            [Description("Ret.Packaging Issue")]
            LN = 71,

            [Description("Scheduling Agreement")]
            LP = 72,

            [Description("Deb.MemoReq. f.Ctrct")]
            LV = 73,

            [Description("Sched.Agr.w / Rel.")]
            LZ = 74,

            [Description("SchedAgrt w/Dly Ord.")]
            LZM = 75,

            [Description("SA:Self-bill w/Inv.")]
            LZS = 76,

            [Description("Dely Order Correctn")]
            MAKO = 77,

            [Description("Rental Contract")]
            MV = 78,

            [Description("Replenishment Dlv.")]
            NL = 79,

            [Description("OBSS")]
            OBSS = 80,

            [Description("Standard Order")]
            OR = 81,

            [Description("Standard Order")]
            OR1 = 82,

            [Description("Pendulum List Req")]
            PLPA = 83,

            [Description("Pendulum List Ret.")]
            PLPR = 84,

            [Description("Pendulum List Cancel")]
            PLPS = 85,

            [Description("Item Proposal")]
            PV = 86,

            [Description("Quotation")]
            QT = 87,

            [Description("Repair Request")]
            RA = 88,

            [Description("ARM In-House Repair")]
            RA2 = 89,

            [Description("Stock Inquiry")]
            RAF = 90,

            [Description("Repairs/Service")]
            RAS = 91,

            [Description("Returns")]
            RE = 92,

            [Description("Advanced Returns")]
            RE2 = 93,

            [Description("Invoice Correct. Req")]
            RK = 94,

            [Description("Delvy Order Returns")]
            RM = 95,

            [Description("SPE Return to Cust.")]
            RTTC = 96,

            [Description("SPE Return Over")]
            RTTR = 97,

            [Description("ARM Ext. Repair Ord")]
            RX2 = 98,

            [Description("Returns Sched.Agrmnt")]
            RZ = 99,

            [Description("Cre.MemoReq. f.Srv")]
            SCR = 100,



            [Description("Subs.Dlv.Free of Ch.")]
            SD = 101,

            [Description("ARM SDF")]
            SD2 = 102,

            [Description("Sales Information")]
            SI = 103,

            [Description("Rush Order")]
            SO = 104,

            [Description("Sales Order (Srvce)")]
            SRVO = 105,

            [Description("Order in Soln Qtan")]
            SRVP = 106,

            [Description("Inquiry")]
            STAT = 107,

            [Description("Standard Order (FPl)")]
            TAF = 108,

            [Description("Delivery Order")]
            TAM = 109,

            [Description("Standard Order (VMI)")]
            TAV = 110,

            [Description("Telephone Sales")]
            TSA = 111,

            [Description("Used Parts Returns")]
            UPRR = 112,

            [Description("New Parts Returns")]
            UUPR = 113,

            [Description("Rel. to Value Contr")]
            WA = 114,

            [Description("Value Contract- Gen")]
            WK1 = 115,

            [Description("Matl-rel. Value Cont")]
            WK2 = 116,

            [Description("WM Prod.Supply")]
            WMPP = 117,

            [Description("Service and Maint")]
            WV = 118,

            [Description("Pallet Crdt Memo-SEP")]
            YCP = 119,

            [Description("Credit Memo Req-SEP")]
            YCR = 120,

            [Description("Contr Rel Ord-SEP")]
            YCRO = 121,

            [Description("Delivered Order-Bank")]
            YDBI = 122,

            [Description("Delivered Order-SEP")]
            YBBO = 123,

            [Description("Depot Sales Order")]
            YBEP = 124,

            [Description("DOM Child Order")]
            YDOR = 125,

            [Description("Debit Memo Req-SEP")]
            YDR = 126,

            [Description("Rebate Credit Memo")]
            YIB1 = 127,

            [Description("Rebate Part Settlmnt")]
            YIB3 = 128,

            [Description("Rebate Manual Accrls")]
            YIB4 = 129,

            [Description("PR/Free Child Order")]
            YPR = 130,

            [Description("PR Free Sales-SEP")]
            YPR1 = 131,

            [Description("Return Sales Ord-SEP")]
            YRE = 132,

            [Description("Self Col Order- Bank")]
            YSB1 = 133,

            [Description("Collect Order-SEP")]
            YSBO = 134,

            [Description("Self ColSmalCustBank")]
            YSMI = 135,

            [Description("Self Col Sales Order")]
            YSOR = 136,

            [Description("Selfcol  SmalCusBank")]
            YSSI = 137,

            [Description("EXP Child Order")]
            YXDB = 138,

            [Description("Asset Sales Order")]
            ZAST = 139,

            [Description("Bonus Credit Request")]
            ZBON = 140,

            [Description("DCP Contract")]
            ZCQ = 141,

            [Description("Return Credit Reqst.")]
            ZCR = 142,

            [Description("Contr Release Order")]
            ZCRO = 143,

            [Description("Delivered Head- Bank")]
            ZDBI = 144,

            [Description("DOM Bulk Order")]
            ZDBO = 145,

            [Description("Del Bulk Order- Bank")]
            ZDCI = 146,

            [Description("Depot Order - Head")]
            ZDEP = 147,

            [Description("OLD Delivered SO")]
            ZDOL = 148,

            [Description("DOM Parent Order")]
            ZDOR = 149,

            [Description("Free Issues-Logistic")]
            ZDPR = 150,

            [Description("Return Debit Reqst")]
            ZDR = 151,

            [Description("DCP Export Order")]
            ZEXP = 152,

            [Description("Freight Request")]
            ZFR = 153,

            [Description("Freight Refund Rqst")]
            ZFRE = 154,

            [Description("Inquiry")]
            ZIN = 155,

            [Description("Sals Docfor MrktngPR")]
            ZMPR = 156,

            [Description("PR/Free Parent Order")]
            ZPR = 157,

            [Description("Quotation")]
            ZQT = 158,

            [Description("Return Order")]
            ZRE = 159,

            [Description("Accident-Returns")]
            ZREA = 160,
    }
    

}

