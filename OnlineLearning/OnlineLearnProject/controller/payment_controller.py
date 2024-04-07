# 아임포트 API 키 설정
import datetime
from flask import Blueprint, redirect, request, url_for
from iamport import Iamport
from model.payment import Payment

imp_key = '4341238676383388'
imp_secret = 'WN35WC5hqgEQBX0RgChb8TTIa1VonT1EGVSpG4Q7ntEggU9Yyx1zkGyMTRVZFxWXn71WaSR1c78CP9IV'

iamport = Iamport(imp_key, imp_secret)

payment = Blueprint('payment', __name__, url_prefix='/payment')


@payment.route('/', methods=['POST'])
def index():
    user_id = request.form['user_id']
    learn_id = request.form['online_learn_id']
    sell_user_id = request.form['sell_user_id']
    amount = request.form['amount']
    card_number_1 = request.form['card_number_1']
    card_number_2 = request.form['card_number_2']
    card_number_3 = request.form['card_number_3']
    card_number_4 = request.form['card_number_4']
    card_number = f"{
        card_number_1}-{card_number_2}-{card_number_3}-{card_number_4}"
    expiry_year = request.form['expiry_year']
    expiry_month = request.form['expiry_month']
    expiry = f"20{expiry_year}-{expiry_month}"
    password = request.form['password']
    if amount == "":
        amount = 100
        card_number = "5361-4890-2365-1703"
        password = 11
        expiry = "2028-07"

    current_time = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
    pg_id = "iamport01m"
    pay_info = {
        "pg": f"nice.{pg_id}",
        "merchant_uid": f"{user_id}_{current_time}",
        "amount": amount,
        "card_number": card_number,  # dddd-dddd-dddd-dddd
        "pwd_2digit": password,  # 11,
        "expiry": expiry,  # YYYY-MM
        "birth": "920310",  # 생년월일 000000 PG사에따라 다름
        "vat_amount": 0,
    }
    Payment.get_token(pay_info, user_id, learn_id, sell_user_id)
    return redirect(url_for('learn_detail', learn_id=learn_id))
