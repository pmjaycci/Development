var IMP = window.IMP;
IMP.init("store-58d513cc-2fc8-432e-83f0-8b98a6cb5574"); // 아임포트 가맹점 코드

function requestPay(learnId) {
    IMP.request_pay({
        pg: "html5_inicis",
        pay_method: "card",
        merchant_uid: learnId, // 강의 ID를 주문 번호로 사용
        name: "{{ learn_detail['title'] }}", // 강의 제목
        amount: 1000, // 실제 금액 설정
        buyer_email: "test@example.com",
        buyer_name: "구매자이름",
        buyer_tel: "010-1234-5678",
    }, function (resp) {
        if (resp.success) {
            $.post("/payment/complete", resp, function (data) {
                if (data == "ok") {
                    alert("결제가 완료되었습니다.");
                } else {
                    alert("결제 검증에 실패했습니다. 다시 시도해 주세요.");
                }
            });
        } else {
            alert("결제에 실패했습니다. 에러 내용: " + resp.error_msg);
        }
    });
}