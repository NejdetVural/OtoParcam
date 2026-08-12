import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getUsers, promoteUser, demoteUser, type AdminUserDto } from "../api/adminUsers";
import { extractErrorMessages } from "../api/errors";
import { useAuth } from "../auth/AuthContext";
import { Badge } from "../components/ui/Badge";
import { ConfirmButton } from "../components/ui/ConfirmButton";
import { Input } from "../components/ui/Input";
import { Spinner } from "../components/ui/Spinner";

function formatDate(iso: string): string {
  const date = new Date(iso);
  if (date.getFullYear() <= 1) {
    // Accounts created before CreatedAt auto-stamping covered ApplicationUser have no real value on record.
    return "Bilinmiyor";
  }
  return date.toLocaleDateString("tr-TR", { year: "numeric", month: "long", day: "numeric" });
}

function UserRow({ user, isSelf }: { user: AdminUserDto; isSelf: boolean }) {
  const queryClient = useQueryClient();
  const [error, setError] = useState<string | null>(null);

  const promoteMutation = useMutation({
    mutationFn: () => promoteUser(user.id),
    onSuccess: () => {
      setError(null);
      queryClient.invalidateQueries({ queryKey: ["admin-users"] });
    },
    onError: (err) => setError(extractErrorMessages(err)[0]),
  });

  const demoteMutation = useMutation({
    mutationFn: () => demoteUser(user.id),
    onSuccess: () => {
      setError(null);
      queryClient.invalidateQueries({ queryKey: ["admin-users"] });
    },
    onError: (err) => setError(extractErrorMessages(err)[0]),
  });

  return (
    <div className="flex flex-col gap-3 rounded-xl border border-slate-200 bg-white shadow-sm p-5">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <p className="text-sm font-medium text-slate-900">
            {user.firstName} {user.lastName}
            {isSelf && <span className="ml-2 text-xs font-normal text-slate-400">(siz)</span>}
          </p>
          <p className="text-xs text-slate-500">{user.email}</p>
          <p className="text-xs text-slate-500">{user.phoneNumber}</p>
          <p className="mt-1 text-xs text-slate-400">Kayıt: {formatDate(user.createdAt)}</p>
        </div>
        <div className="flex flex-col items-end gap-1.5">
          <Badge tone={user.isAdministrator ? "success" : "neutral"}>
            {user.isAdministrator ? "Yönetici" : "Müşteri"}
          </Badge>
          <Badge tone={user.emailConfirmed ? "success" : "warning"}>
            {user.emailConfirmed ? "E-posta Onaylı" : "E-posta Onaysız"}
          </Badge>
        </div>
      </div>

      <div className="flex flex-wrap items-center gap-4 border-t border-slate-100 pt-3 text-xs text-slate-500">
        <span>{user.favoriteCount} favori</span>
        <span>{user.purchaseRequestCount} satın alma talebi</span>
      </div>

      {error && <p className="text-sm text-red-600">{error}</p>}

      <div className="flex items-center justify-end border-t border-slate-100 pt-3">
        {user.isAdministrator ? (
          <ConfirmButton
            label="Yöneticilikten Çıkar"
            confirmLabel="Evet, Çıkar"
            message="Bu kullanıcının yönetici yetkisi kaldırılsın mı?"
            disabled={isSelf || demoteMutation.isPending}
            onConfirm={() => demoteMutation.mutate()}
            triggerVariant="secondary"
          />
        ) : (
          <ConfirmButton
            label="Yönetici Yap"
            confirmLabel="Evet, Yönetici Yap"
            message="Bu kullanıcı yönetici yapılsın mı?"
            disabled={promoteMutation.isPending}
            onConfirm={() => promoteMutation.mutate()}
            triggerVariant="secondary"
          />
        )}
      </div>
    </div>
  );
}

export function AdminUsersPage() {
  const { user: currentUser } = useAuth();
  const [keyword, setKeyword] = useState("");

  const usersQuery = useQuery({
    queryKey: ["admin-users", keyword],
    queryFn: () => getUsers({ keyword: keyword || undefined }),
  });

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold text-slate-900">Kullanıcı Yönetimi</h1>
          <p className="text-sm text-slate-500">Kayıtlı kullanıcıları görüntüleyin ve yöneticilik yetkisi atayın.</p>
        </div>
        <Input
          placeholder="Ad, e-posta veya telefon ara"
          value={keyword}
          onChange={(e) => setKeyword(e.target.value)}
          className="w-64"
        />
      </div>

      {usersQuery.isLoading && (
        <div className="flex justify-center py-16">
          <Spinner />
        </div>
      )}

      {usersQuery.isError && (
        <p className="rounded-lg border border-red-100 bg-red-50 p-4 text-sm text-red-700">
          Kullanıcılar yüklenirken bir hata oluştu.
        </p>
      )}

      {usersQuery.data &&
        (usersQuery.data.length === 0 ? (
          <div className="rounded-xl border border-dashed border-slate-300 bg-white p-12 text-center">
            <p className="text-sm text-slate-500">Bu kriterlere uygun kullanıcı bulunamadı.</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            {usersQuery.data.map((user) => (
              <UserRow key={user.id} user={user} isSelf={user.id === currentUser?.id} />
            ))}
          </div>
        ))}
    </div>
  );
}
