# 아임포트 API 키 설정
import datetime
from iamport import Iamport
import requests
from model.database import Database

imp_key = '4341238676383388'
imp_secret = 'WN35WC5hqgEQBX0RgChb8TTIa1VonT1EGVSpG4Q7ntEggU9Yyx1zkGyMTRVZFxWXn71WaSR1c78CP9IV'

iamport = Iamport(imp_key, imp_secret)


class Payment:
    def get_token(payload, user_id, learn_id, sell_user_id):
        access_data = {
            "imp_key": imp_key,
            "imp_secret": imp_secret,
        }
        print("--------------------------------")
        print(f'TEST::\n{payload}')
        print("--------------------------------")
        req = requests.post(
            'https://api.iamport.kr/users/getToken', data=access_data)
        res = req.json()
        # print(res)
        access_token = res['response']['access_token']
        Payment.one_time_buy(access_token, payload,
                             user_id, learn_id, sell_user_id)
        pass

    def one_time_buy(access_token, info, user_id, learn_id, sell_user_id):
        # stor_id = "store-58d513cc-2fc8-432e-83f0-8b98a6cb5574"

        header = {
            "Content-Type": "application/json",
            "Authorization": f'Bearer {access_token}'

        }
        req = requests.post(
            'https://api.iamport.kr/subscribe/payments/onetime', json=info, headers=header)
        res = req.json()
        if res["code"] == 0:
            current_time = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
            response = res["response"]
            amount = response['amount']
            # 'https://npg.nicepay.co.kr/issue/IssueLoader.do?TID=iamport01m01162403290714307137&type=0&InnerWin=Y',
            receipt_url = response['receipt_url']
            card_name = response["card_name"]

            sql = f'INSERT INTO buy_online_learn (user_id, online_learn_id, amount, card_name, receipt_url, created_at) VALUES(?,?,?,?,?,?)'
            Database.write_db(sql, user_id, learn_id, amount,
                              card_name, receipt_url, current_time)
            sql = f'INSERT INTO sell_online_learn (user_id, online_learn_id, created_at) VALUES(?,?,?)'
            Database.write_db(sql, sell_user_id, learn_id, current_time)

            return

        print(res)
        return


'''
{
    'code': 0, 
    'message': None, 
    'response': {
        'amount': 100, 
        'apply_num': '50769994', 
        'bank_code': None, 
        'bank_name': None, 
        'buyer_addr': None,
        'buyer_email': None,
        'buyer_name': None,
        'buyer_postcode': None,
        'buyer_tel': None,
        'cancel_amount': 0,
        'cancel_history': [],
        'cancel_reason': None,
        'cancel_receipt_urls': [],
        'cancelled_at': 0,
        'card_code': '365',
        'card_name': '삼성카드',
        'card_number': '53614890****1703',
        'card_quota': 0,
        'card_type': 0,
        'cash_receipt_issued': False,
        'channel': 'api',
        'currency': 'KRW',
        'custom_data': None,
        'customer_uid': None,
        'customer_uid_usage': None,
        'emb_pg_provider': None,
        'escrow': False,
        'fail_reason': None,
        'failed_at': 0,
        'imp_uid': 'imps_504070720134',
        'merchant_uid': 'test_2024-03-29 07:14:30',
        'name': None, 'paid_at': 1711664071,
        'pay_method': 'card',
        'pg_id': 'iamport01m',
        'pg_provider': 'nice',
        'pg_tid': 'iamport01m01162403290714307137',
        'receipt_url': 'https://npg.nicepay.co.kr/issue/IssueLoader.do?TID=iamport01m01162403290714307137&type=0&InnerWin=Y',
        'started_at': 1711664070,
        'status': 'paid',
        'user_agent': 'python-requests/2.26.0',
        'vbank_code': None,
        'vbank_date': 0,
        'vbank_holder': None,
        'vbank_issued_at': 0,
        'vbank_name': None,
        'vbank_num': None}
        }
'''
