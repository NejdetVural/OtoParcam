import { useEffect, useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { getMyProfile, updateMyProfile } from "../api/users";
import { getFavorites } from "../api/favorites";
import { getPurchaseRequests, PurchaseRequestStatus } from "../api/purchaseRequests";
import { extractErrorMessages } from "../api/errors";
import { useAuth } from "../auth/AuthContext";
import { Roles } from "../auth/roles";
import { Button } from "../components/ui/Button";
import { Input } from "../components/ui/Input";
import { Spinner } from "../components/ui/Spinner";
import { Badge } from "../components/ui/Badge";

function initials(firstName: string, lastName: string): string {
  return `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase();
}

export function ProfilePage() {
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const isAdmin = user?.roles.includes(Roles.Administrator) ?? false;
  const profileQuery = useQuery({ queryKey: ["my-profile"], queryFn: getMyProfile });
  const favoritesQuery = useQuery({ queryKey: ["favorites"], queryFn: getFavorites, enabled: !isAdmin });
  const purchaseRequestsQuery = useQuery({
    queryKey: ["purchase-requests"],
    queryFn: getPurchaseRequests,
    enabled: !isAdmin,
  });

  const [form, setForm] = useState({ firstName: "", lastName: "" });
  const [errors, setErrors] = useState<string[]>([]);
  const [savedAt, setSavedAt] = useState<number | null>(null);

  useEffect(() => {
    if (profileQuery.data) {
      setForm({ firstName: profileQuery.data.firstName, lastName: profileQuery.data.lastName });
    }
  }, [profileQuery.data]);

  const updateMutation = useMutation({
    mutationFn: updateMyProfile,
    onSuccess: (profile) => {
      queryClient.setQueryData(["my-profile"], profile);
      setSavedAt(Date.now());
    },
    onError: (error) => setErrors(extractErrorMessages(error)),
  });

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setErrors([]);
    setSavedAt(null);
    updateMutation.mutate(form);
  }

  if (profileQuery.isLoading) {
    return (
      <div className="flex justify-center py-16">
        <Spinner />
      </div>
    );
  }

  if (profileQuery.isError || !profileQuery.data) {
    return (
      <p className="rounded-lg border border-red-100 bg-red-50 p-4 text-sm text-red-700">
        Profil bilgileri yüklenirken bir hata oluştu.
      </p>
    );
  }

  const profile = profileQuery.data;
  const pendingRequestCount = purchaseRequestsQuery.data?.filter(
    (r) => r.status === PurchaseRequestStatus.Pending || r.status === PurchaseRequestStatus.WaitingForCustomerConfirmation,
  ).length;

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-col gap-4 rounded-xl border border-slate-200 bg-white shadow-sm p-6 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex items-center gap-4">
          <div className="flex h-14 w-14 items-center justify-center rounded-full bg-brand-800 text-lg font-semibold text-white">
            {initials(profile.firstName, profile.lastName)}
          </div>
          <div>
            <div className="flex items-center gap-2">
              <h1 className="text-xl font-semibold text-slate-900">
                {profile.firstName} {profile.lastName}
              </h1>
              <Badge>{isAdmin ? "Yönetici" : "Müşteri"}</Badge>
            </div>
            <p className="text-sm text-slate-500">{profile.email}</p>
            <p className="text-sm text-slate-500">{profile.phoneNumber}</p>
          </div>
        </div>
      </div>

      {!isAdmin && (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <Link
            to="/favoriler"
            className="flex items-center justify-between rounded-xl border border-slate-200 bg-white shadow-sm p-5 transition-shadow hover:shadow-md"
          >
            <div>
              <p className="text-sm text-slate-500">Favori Ürünler</p>
              <p className="text-2xl font-semibold text-slate-900">
                {favoritesQuery.data ? favoritesQuery.data.length : "—"}
              </p>
            </div>
            <span className="text-sm font-medium text-slate-400">→</span>
          </Link>

          <Link
            to="/taleplerim"
            className="flex items-center justify-between rounded-xl border border-slate-200 bg-white shadow-sm p-5 transition-shadow hover:shadow-md"
          >
            <div>
              <p className="text-sm text-slate-500">Bekleyen Talepler</p>
              <p className="text-2xl font-semibold text-slate-900">
                {pendingRequestCount ?? "—"}
                <span className="ml-1 text-sm font-normal text-slate-400">
                  / {purchaseRequestsQuery.data ? purchaseRequestsQuery.data.length : "—"} toplam
                </span>
              </p>
            </div>
            <span className="text-sm font-medium text-slate-400">→</span>
          </Link>
        </div>
      )}

      <div className="max-w-sm">
        <h2 className="mb-3 text-sm font-medium text-slate-700">Hesap Bilgileri</h2>
        <form onSubmit={handleSubmit} className="flex flex-col gap-4 rounded-xl border border-slate-200 bg-white shadow-sm p-6">
          <Input
            label="Ad"
            value={form.firstName}
            onChange={(e) => setForm((p) => ({ ...p, firstName: e.target.value }))}
            required
          />
          <Input
            label="Soyad"
            value={form.lastName}
            onChange={(e) => setForm((p) => ({ ...p, lastName: e.target.value }))}
            required
          />

          <Input label="E-posta" value={profile.email} disabled />
          <Input label="Telefon" value={profile.phoneNumber} disabled />
          <p className="-mt-3 text-xs text-slate-500">E-posta ve telefon numarası bu sürümde değiştirilemez.</p>

          {errors.length > 0 && (
            <ul className="flex flex-col gap-1 rounded-lg border border-red-100 bg-red-50 p-3 text-sm text-red-700">
              {errors.map((message) => (
                <li key={message}>{message}</li>
              ))}
            </ul>
          )}

          {savedAt && !updateMutation.isPending && (
            <p className="rounded-lg border border-emerald-100 bg-emerald-50 p-3 text-sm text-emerald-700">
              Bilgileriniz güncellendi.
            </p>
          )}

          <Button type="submit" disabled={updateMutation.isPending}>
            {updateMutation.isPending ? "Kaydediliyor…" : "Kaydet"}
          </Button>
        </form>
      </div>
    </div>
  );
}
