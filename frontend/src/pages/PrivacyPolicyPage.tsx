import type { ReactNode } from "react";

function Section({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="flex flex-col gap-2">
      <h2 className="text-base font-semibold text-slate-900">{title}</h2>
      <div className="flex flex-col gap-2 text-sm leading-relaxed text-slate-600">{children}</div>
    </section>
  );
}

export function PrivacyPolicyPage() {
  return (
    <div className="mx-auto flex max-w-3xl flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold text-slate-900">Gizlilik Politikası</h1>
        <p className="text-sm text-slate-500">Kişisel Verilerin Korunması Hakkında Aydınlatma Metni</p>
      </div>

      <div className="rounded-lg border border-amber-200 bg-amber-50 p-4 text-sm text-amber-800">
        Bu sayfa, gerçek işletme bilgileri ve bir hukuk danışmanı incelemesiyle tamamlanması gereken bir taslaktır.
        Köşeli parantez içindeki alanlar ([ ]) gerçek şirket bilgileriyle doldurulmalı, metnin bütünü yürürlükteki
        mevzuata (KVKK ve ilgili yönetmelikler) uygunluk açısından bir avukat tarafından gözden geçirilmelidir.
      </div>

      <div className="flex flex-col gap-6 rounded-xl border border-slate-200 bg-white shadow-sm p-6">
        <Section title="1. Veri Sorumlusu">
          <p>
            İşbu Aydınlatma Metni, 6698 sayılı Kişisel Verilerin Korunması Kanunu (&quot;KVKK&quot;) uyarınca, veri
            sorumlusu sıfatıyla <strong>[Şirket Unvanı]</strong> (&quot;OtoParcam&quot; veya &quot;Şirket&quot;)
            tarafından, internet sitemiz ve mobil uygulamamız üzerinden topladığımız kişisel verilerinizin işlenmesine
            ilişkin olarak sizi bilgilendirmek amacıyla hazırlanmıştır.
          </p>
        </Section>

        <Section title="2. Toplanan Kişisel Veriler">
          <p>Hesabınızı oluşturduğunuzda ve hizmetlerimizi kullandığınızda aşağıdaki kişisel verileriniz işlenmektedir:</p>
          <ul className="list-disc pl-5">
            <li>Kimlik bilgileri: ad, soyad</li>
            <li>İletişim bilgileri: e-posta adresi, telefon numarası</li>
            <li>İşlem güvenliği bilgileri: şifrelenmiş parola, oturum/erişim kayıtları</li>
            <li>
              Müşteri işlem bilgileri: favori ürünler, satın alma talepleri, talep geçmişi, pazarlık ve fiyat
              iletişimi kayıtları
            </li>
          </ul>
        </Section>

        <Section title="3. Kişisel Verilerin İşlenme Amaçları">
          <ul className="list-disc pl-5">
            <li>Üyelik/hesap oluşturma ve yönetimi</li>
            <li>Satın alma taleplerinizin oluşturulması, değerlendirilmesi ve sonuçlandırılması</li>
            <li>Müşteri ilişkileri yönetimi ve talepleriniz hakkında sizinle iletişime geçilmesi</li>
            <li>Yasal yükümlülüklerin (vergi, muhasebe, tüketici mevzuatı vb.) yerine getirilmesi</li>
            <li>Hizmet kalitesinin ve site güvenliğinin sağlanması</li>
          </ul>
        </Section>

        <Section title="4. Kişisel Verilerin Toplanma Yöntemi ve Hukuki Sebebi">
          <p>
            Kişisel verileriniz, internet sitemiz üzerinden doldurduğunuz formlar aracılığıyla elektronik ortamda
            toplanmaktadır. Veriler; bir sözleşmenin kurulması veya ifasıyla doğrudan doğruya ilgili olması, hukuki
            yükümlülüğün yerine getirilmesi ve açık rızanızın bulunması hukuki sebeplerine dayanılarak işlenmektedir
            (KVKK m. 5).
          </p>
        </Section>

        <Section title="5. Kişisel Verilerin Aktarılması">
          <p>
            Kişisel verileriniz, yasal zorunluluklar dışında, açık rızanız olmaksızın üçüncü kişilerle
            paylaşılmamaktadır. Yasal yükümlülüklerimiz gereği yetkili kamu kurum ve kuruluşları ile, hizmet aldığımız
            tedarikçilerle (ör. barındırma/hosting sağlayıcısı) sınırlı şekilde paylaşım yapılabilir.
          </p>
        </Section>

        <Section title="6. Kişisel Verilerin Saklanma Süresi">
          <p>
            Kişisel verileriniz, ilgili mevzuatta öngörülen süreler ve/veya işleme amacının gerektirdiği süre boyunca
            saklanmakta, bu sürelerin sonunda silinmekte, yok edilmekte veya anonim hale getirilmektedir.
          </p>
        </Section>

        <Section title="7. KVKK Kapsamındaki Haklarınız">
          <p>KVKK&apos;nın 11. maddesi uyarınca, kişisel verilerinize ilişkin olarak:</p>
          <ul className="list-disc pl-5">
            <li>Kişisel veri işlenip işlenmediğini öğrenme,</li>
            <li>İşlenmişse buna ilişkin bilgi talep etme,</li>
            <li>İşlenme amacını ve amacına uygun kullanılıp kullanılmadığını öğrenme,</li>
            <li>Yurt içinde/dışında aktarıldığı üçüncü kişileri bilme,</li>
            <li>Eksik/yanlış işlenmişse düzeltilmesini isteme,</li>
            <li>KVKK m. 7&apos;deki şartlar çerçevesinde silinmesini/yok edilmesini isteme,</li>
            <li>Bu işlemlerin, verilerin aktarıldığı üçüncü kişilere bildirilmesini isteme,</li>
            <li>Otomatik sistemlerle analiz sonucu aleyhinize bir sonucun ortaya çıkmasına itiraz etme,</li>
            <li>Kanuna aykırı işleme nedeniyle zarara uğramanız hâlinde zararın giderilmesini talep etme</li>
          </ul>
          <p>haklarına sahipsiniz.</p>
        </Section>

        <Section title="8. İletişim">
          <p>
            Haklarınızı kullanmak veya sorularınız için bizimle <strong>[İletişim E-posta Adresi]</strong> üzerinden
            iletişime geçebilirsiniz.
          </p>
        </Section>

        <Section title="9. Değişiklikler">
          <p>
            Bu Aydınlatma Metni, yasal düzenlemeler veya işleme faaliyetlerimizdeki değişiklikler doğrultusunda
            güncellenebilir. Güncel metin her zaman bu sayfada yayınlanır.
          </p>
        </Section>

        <p className="text-xs text-slate-400">Son güncelleme: [Tarih]</p>
      </div>
    </div>
  );
}
