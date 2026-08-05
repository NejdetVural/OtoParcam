import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { addFavorite, getFavorites, removeFavorite } from "../../api/favorites";
import { useAuth } from "../../auth/AuthContext";
import { Roles } from "../../auth/roles";

export function FavoriteButton({ productId, className = "" }: { productId: string; className?: string }) {
  const { isAuthenticated, user } = useAuth();
  const isAdmin = user?.roles.includes(Roles.Administrator) ?? false;
  const queryClient = useQueryClient();

  const favoritesQuery = useQuery({
    queryKey: ["favorites"],
    queryFn: getFavorites,
    enabled: isAuthenticated && !isAdmin,
  });
  const isFavorite = favoritesQuery.data?.some((product) => product.id === productId) ?? false;

  const toggleMutation = useMutation({
    mutationFn: () => (isFavorite ? removeFavorite(productId) : addFavorite(productId)),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["favorites"] }),
  });

  if (!isAuthenticated || isAdmin) {
    return null;
  }

  return (
    <button
      type="button"
      onClick={(e) => {
        e.preventDefault();
        e.stopPropagation();
        toggleMutation.mutate();
      }}
      disabled={toggleMutation.isPending}
      aria-label={isFavorite ? "Favorilerden çıkar" : "Favorilere ekle"}
      aria-pressed={isFavorite}
      className={`inline-flex h-9 w-9 items-center justify-center rounded-full border transition-colors disabled:opacity-60 ${
        isFavorite
          ? "border-red-200 bg-red-50 text-red-500"
          : "border-slate-200 bg-white text-slate-400 hover:text-slate-600"
      } ${className}`}
    >
      <svg viewBox="0 0 24 24" className="h-5 w-5" fill={isFavorite ? "currentColor" : "none"} stroke="currentColor" strokeWidth={1.8}>
        <path d="M12 21s-6.716-4.35-9.428-8.03C.86 10.42 1.2 6.91 4.05 5.2A5.5 5.5 0 0112 6.06 5.5 5.5 0 0119.95 5.2c2.85 1.71 3.19 5.22 1.478 7.77C18.716 16.65 12 21 12 21z" />
      </svg>
    </button>
  );
}
