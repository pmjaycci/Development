
from flask import Blueprint, redirect, request, session, url_for
from model.account import Account

account = Blueprint('account', __name__, url_prefix='/account')


@account.route('/login', methods=['POST'])
def login():
    username = request.form['username']
    password = request.form['password']
    Account.login_check(username, password)
    print(f"Name [{username}]/[{password}]")
    return redirect(url_for('index'))


@account.route('/logout', methods=['GET'])
def logout():
    session.pop('user_id', None)
    return redirect(url_for('index'))
