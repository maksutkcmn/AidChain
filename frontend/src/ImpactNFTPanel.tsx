import { useState, useEffect } from 'react';
import { useCurrentAccount, useSignAndExecuteTransaction, useSuiClient } from '@mysten/dapp-kit';
import { Transaction } from '@mysten/sui/transactions';
import { AIDCHAIN_PACKAGE_ID, IMPACT_COUNTER_ID, IMPACT_COUNTER_INITIAL_SHARED_VERSION } from './config';
import { useSponsoredTransaction } from './useSponsoredTransaction';

interface ImpactNFT {
  id: string;
  name: string;
  description: string;
  imageUrl: string;
  donationAmount: number;
  recipientLocation: string;
  recipientName: string;
  tier: number;
  tierName: string;
  donatedAtEpoch: number;
  impactId: number;
}

const TIER_COLORS: Record<number, { bg: string; border: string; text: string; glow: string }> = {
  1: { bg: 'linear-gradient(135deg, #CD7F32 0%, #8B4513 100%)', border: '#CD7F32', text: '#FFF', glow: '0 0 20px rgba(205, 127, 50, 0.5)' },
  2: { bg: 'linear-gradient(135deg, #C0C0C0 0%, #808080 100%)', border: '#C0C0C0', text: '#FFF', glow: '0 0 20px rgba(192, 192, 192, 0.5)' },
  3: { bg: 'linear-gradient(135deg, #FFD700 0%, #DAA520 100%)', border: '#FFD700', text: '#000', glow: '0 0 20px rgba(255, 215, 0, 0.5)' },
  4: { bg: 'linear-gradient(135deg, #B9F2FF 0%, #00CED1 100%)', border: '#00CED1', text: '#000', glow: '0 0 30px rgba(0, 206, 209, 0.7)' },
};

const TIER_ICONS: Record<number, string> = {
  1: '🥉',
  2: '🥈',
  3: '🥇',
  4: '💎',
};

const TIER_THRESHOLDS = {
  bronze: 0.1,
  silver: 1,
  gold: 10,
  diamond: 100,
};

export function ImpactNFTPanel() {
  const currentAccount = useCurrentAccount();
  const client = useSuiClient();
  const { mutate: signAndExecute } = useSignAndExecuteTransaction();
  const { executeSponsored, isEnabled: sponsoredEnabled } = useSponsoredTransaction();

  const [myNFTs, setMyNFTs] = useState<ImpactNFT[]>([]);
  const [loading, setLoading] = useState(false);
  const [minting, setMinting] = useState(false);
  const [message, setMessage] = useState('');
  const [totalImpacts, setTotalImpacts] = useState(0);
  const [useSponsored, setUseSponsored] = useState(true);

  // Mint form state
  const [mintAmount, setMintAmount] = useState('');
  const [mintLocation, setMintLocation] = useState('');
  const [mintRecipient, setMintRecipient] = useState('');

  useEffect(() => {
    if (currentAccount) {
      loadMyNFTs();
      loadTotalImpacts();
    }
  }, [currentAccount]);

  const loadMyNFTs = async () => {
    if (!currentAccount) return;
    setLoading(true);

    try {
      const objects = await client.getOwnedObjects({
        owner: currentAccount.address,
        filter: {
          StructType: `${AIDCHAIN_PACKAGE_ID}::impact_nft::ImpactNFT`,
        },
        options: { showContent: true, showDisplay: true },
      });

      const nfts: ImpactNFT[] = [];
      for (const obj of objects.data) {
        if (obj.data?.content?.dataType === 'moveObject') {
          const fields = obj.data.content.fields as any;
          nfts.push({
            id: obj.data.objectId,
            name: fields.name,
            description: fields.description,
            imageUrl: fields.image_url,
            donationAmount: parseInt(fields.donation_amount) / 1_000_000_000,
            recipientLocation: fields.recipient_location,
            recipientName: fields.recipient_name,
            tier: parseInt(fields.tier),
            tierName: fields.tier_name,
            donatedAtEpoch: parseInt(fields.donated_at_epoch),
            impactId: parseInt(fields.impact_id),
          });
        }
      }

      // Sort by impact ID (newest first)
      nfts.sort((a, b) => b.impactId - a.impactId);
      setMyNFTs(nfts);
    } catch (error) {
      console.error('Failed to load NFTs:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadTotalImpacts = async () => {
    try {
      const obj = await client.getObject({
        id: IMPACT_COUNTER_ID,
        options: { showContent: true },
      });

      if (obj.data?.content?.dataType === 'moveObject') {
        const fields = obj.data.content.fields as any;
        setTotalImpacts(parseInt(fields.count));
      }
    } catch (error) {
      console.error('Failed to load total impacts:', error);
    }
  };

  const getTierFromAmount = (amount: number): { tier: number; name: string } => {
    if (amount >= TIER_THRESHOLDS.diamond) return { tier: 4, name: 'Diamond' };
    if (amount >= TIER_THRESHOLDS.gold) return { tier: 3, name: 'Gold' };
    if (amount >= TIER_THRESHOLDS.silver) return { tier: 2, name: 'Silver' };
    return { tier: 1, name: 'Bronze' };
  };

  const handleMintNFT = async () => {
    if (!currentAccount) {
      setMessage('Please connect your wallet first');
      return;
    }

    const amount = parseFloat(mintAmount);
    if (!amount || amount < 0.1) {
      setMessage('Minimum donation amount is 0.1 SUI');
      return;
    }

    if (!mintLocation.trim()) {
      setMessage('Please enter recipient location');
      return;
    }

    setMinting(true);
    setMessage('');

    try {
      const amountInMist = Math.floor(amount * 1_000_000_000);

      const txb = new Transaction();
      txb.moveCall({
        target: `${AIDCHAIN_PACKAGE_ID}::impact_nft::mint_impact_nft`,
        arguments: [
          txb.sharedObjectRef({
            objectId: IMPACT_COUNTER_ID,
            initialSharedVersion: IMPACT_COUNTER_INITIAL_SHARED_VERSION,
            mutable: true,
          }),
          txb.pure.u64(amountInMist),
          txb.pure.string(mintLocation),
          txb.pure.string(mintRecipient || 'Anonymous'),
        ],
      });

      if (useSponsored && sponsoredEnabled) {
        setMessage('⛽ Minting NFT with sponsored gas...');
        const result = await executeSponsored(txb, [
          `${AIDCHAIN_PACKAGE_ID}::impact_nft::mint_impact_nft`,
        ]);

        if (result.success) {
          setMessage('🎉 Impact NFT minted successfully!');
          setMintAmount('');
          setMintLocation('');
          setMintRecipient('');
          await loadMyNFTs();
          await loadTotalImpacts();
        } else {
          setMessage(`Mint failed: ${result.error}`);
        }
      } else {
        signAndExecute(
          { transaction: txb },
          {
            onSuccess: async () => {
              setMessage('🎉 Impact NFT minted successfully!');
              setMintAmount('');
              setMintLocation('');
              setMintRecipient('');
              await loadMyNFTs();
              await loadTotalImpacts();
            },
            onError: (err: any) => {
              setMessage(`Mint failed: ${err.message}`);
            },
          }
        );
      }
    } catch (error: any) {
      setMessage(`Error: ${error.message}`);
    } finally {
      setMinting(false);
    }
  };

  const tierInfo = mintAmount ? getTierFromAmount(parseFloat(mintAmount) || 0) : null;

  return (
    <div style={{ padding: '20px', maxWidth: '1200px', margin: '0 auto' }}>
      <h2 style={{ textAlign: 'center', marginBottom: '10px' }}>
        🏆 Proof of Impact NFTs
      </h2>
      <p style={{ textAlign: 'center', color: '#666', marginBottom: '30px' }}>
        Mint your donation certificate as an NFT • Total Impacts: <strong>{totalImpacts}</strong>
      </p>

      {/* Tier Legend */}
      <div style={{
        display: 'flex',
        justifyContent: 'center',
        gap: '20px',
        marginBottom: '30px',
        flexWrap: 'wrap',
      }}>
        {[
          { tier: 1, name: 'Bronze', min: '0.1 SUI' },
          { tier: 2, name: 'Silver', min: '1 SUI' },
          { tier: 3, name: 'Gold', min: '10 SUI' },
          { tier: 4, name: 'Diamond', min: '100 SUI' },
        ].map(t => (
          <div key={t.tier} style={{
            padding: '8px 16px',
            borderRadius: '20px',
            background: TIER_COLORS[t.tier].bg,
            color: TIER_COLORS[t.tier].text,
            fontSize: '14px',
            fontWeight: 'bold',
          }}>
            {TIER_ICONS[t.tier]} {t.name} ({t.min}+)
          </div>
        ))}
      </div>

      {/* Mint Form */}
      <div style={{
        background: '#f8f9fa',
        padding: '25px',
        borderRadius: '16px',
        marginBottom: '30px',
        border: '1px solid #e0e0e0',
      }}>
        <h3 style={{ marginBottom: '20px', color: '#333' }}>🎖️ Mint Your Impact NFT</h3>
        
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '15px' }}>
          <div>
            <label style={{ display: 'block', marginBottom: '5px', fontWeight: '500' }}>
              Donation Amount (SUI) *
            </label>
            <input
              type="number"
              step="0.1"
              min="0.1"
              value={mintAmount}
              onChange={(e) => setMintAmount(e.target.value)}
              placeholder="e.g., 1.0"
              style={{
                width: '100%',
                padding: '10px 12px',
                borderRadius: '8px',
                border: '1px solid #ccc',
                fontSize: '16px',
              }}
            />
            {tierInfo && (
              <div style={{
                marginTop: '8px',
                padding: '6px 12px',
                background: TIER_COLORS[tierInfo.tier].bg,
                color: TIER_COLORS[tierInfo.tier].text,
                borderRadius: '6px',
                fontSize: '13px',
                fontWeight: 'bold',
                display: 'inline-block',
              }}>
                {TIER_ICONS[tierInfo.tier]} You'll get: {tierInfo.name} Tier
              </div>
            )}
          </div>

          <div>
            <label style={{ display: 'block', marginBottom: '5px', fontWeight: '500' }}>
              Recipient Location *
            </label>
            <input
              type="text"
              value={mintLocation}
              onChange={(e) => setMintLocation(e.target.value)}
              placeholder="e.g., Hatay, Turkey"
              style={{
                width: '100%',
                padding: '10px 12px',
                borderRadius: '8px',
                border: '1px solid #ccc',
                fontSize: '16px',
              }}
            />
          </div>

          <div>
            <label style={{ display: 'block', marginBottom: '5px', fontWeight: '500' }}>
              Recipient Name (optional)
            </label>
            <input
              type="text"
              value={mintRecipient}
              onChange={(e) => setMintRecipient(e.target.value)}
              placeholder="Anonymous"
              style={{
                width: '100%',
                padding: '10px 12px',
                borderRadius: '8px',
                border: '1px solid #ccc',
                fontSize: '16px',
              }}
            />
          </div>
        </div>

        {sponsoredEnabled && (
          <div style={{ marginTop: '15px' }}>
            <label style={{ display: 'flex', alignItems: 'center', gap: '8px', cursor: 'pointer' }}>
              <input
                type="checkbox"
                checked={useSponsored}
                onChange={(e) => setUseSponsored(e.target.checked)}
              />
              <span>⛽ Use sponsored gas (free!)</span>
            </label>
          </div>
        )}

        <button
          onClick={handleMintNFT}
          disabled={minting || !mintAmount || !mintLocation}
          style={{
            marginTop: '20px',
            padding: '12px 30px',
            fontSize: '16px',
            fontWeight: 'bold',
            background: minting ? '#ccc' : 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            color: '#fff',
            border: 'none',
            borderRadius: '10px',
            cursor: minting ? 'not-allowed' : 'pointer',
            width: '100%',
          }}
        >
          {minting ? '⏳ Minting...' : '🎖️ Mint Impact NFT'}
        </button>

        {message && (
          <div style={{
            marginTop: '15px',
            padding: '12px',
            borderRadius: '8px',
            background: message.includes('🎉') ? '#d4edda' : '#f8d7da',
            color: message.includes('🎉') ? '#155724' : '#721c24',
            textAlign: 'center',
          }}>
            {message}
          </div>
        )}
      </div>

      {/* My NFTs Gallery */}
      <div>
        <h3 style={{ marginBottom: '20px', color: '#333' }}>
          🖼️ My Impact Collection ({myNFTs.length})
        </h3>

        {loading ? (
          <div style={{ textAlign: 'center', padding: '40px', color: '#666' }}>
            Loading your NFTs...
          </div>
        ) : myNFTs.length === 0 ? (
          <div style={{
            textAlign: 'center',
            padding: '40px',
            background: '#f8f9fa',
            borderRadius: '16px',
            color: '#666',
          }}>
            <p style={{ fontSize: '48px', marginBottom: '10px' }}>🎖️</p>
            <p>You don't have any Impact NFTs yet.</p>
            <p>Mint your first one above!</p>
          </div>
        ) : (
          <div style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))',
            gap: '20px',
          }}>
            {myNFTs.map(nft => (
              <div
                key={nft.id}
                style={{
                  background: '#fff',
                  borderRadius: '16px',
                  overflow: 'hidden',
                  border: `2px solid ${TIER_COLORS[nft.tier]?.border || '#ccc'}`,
                  boxShadow: TIER_COLORS[nft.tier]?.glow || '0 4px 12px rgba(0,0,0,0.1)',
                  transition: 'transform 0.2s',
                }}
                onMouseEnter={(e) => e.currentTarget.style.transform = 'scale(1.02)'}
                onMouseLeave={(e) => e.currentTarget.style.transform = 'scale(1)'}
              >
                {/* NFT Header with Tier Gradient */}
                <div style={{
                  background: TIER_COLORS[nft.tier]?.bg || '#667eea',
                  padding: '20px',
                  textAlign: 'center',
                  color: TIER_COLORS[nft.tier]?.text || '#fff',
                }}>
                  <div style={{ fontSize: '48px', marginBottom: '10px' }}>
                    {TIER_ICONS[nft.tier] || '🏆'}
                  </div>
                  <div style={{ fontSize: '20px', fontWeight: 'bold' }}>
                    {nft.tierName} Tier
                  </div>
                  <div style={{ fontSize: '14px', opacity: 0.9 }}>
                    Impact #{nft.impactId}
                  </div>
                </div>

                {/* NFT Details */}
                <div style={{ padding: '20px' }}>
                  <div style={{ marginBottom: '12px' }}>
                    <div style={{ fontSize: '12px', color: '#666', marginBottom: '2px' }}>
                      Donation Amount
                    </div>
                    <div style={{ fontSize: '18px', fontWeight: 'bold', color: '#333' }}>
                      {nft.donationAmount.toFixed(2)} SUI
                    </div>
                  </div>

                  <div style={{ marginBottom: '12px' }}>
                    <div style={{ fontSize: '12px', color: '#666', marginBottom: '2px' }}>
                      Location
                    </div>
                    <div style={{ fontSize: '14px', color: '#333' }}>
                      📍 {nft.recipientLocation}
                    </div>
                  </div>

                  <div style={{ marginBottom: '12px' }}>
                    <div style={{ fontSize: '12px', color: '#666', marginBottom: '2px' }}>
                      Recipient
                    </div>
                    <div style={{ fontSize: '14px', color: '#333' }}>
                      👤 {nft.recipientName}
                    </div>
                  </div>

                  <div style={{
                    marginTop: '15px',
                    paddingTop: '15px',
                    borderTop: '1px solid #eee',
                    fontSize: '12px',
                    color: '#999',
                  }}>
                    Epoch: {nft.donatedAtEpoch}
                  </div>

                  {/* View on Explorer */}
                  <a
                    href={`https://testnet.suivision.xyz/object/${nft.id}`}
                    target="_blank"
                    rel="noopener noreferrer"
                    style={{
                      display: 'block',
                      marginTop: '10px',
                      padding: '8px',
                      textAlign: 'center',
                      background: '#f0f0f0',
                      borderRadius: '8px',
                      color: '#666',
                      textDecoration: 'none',
                      fontSize: '13px',
                    }}
                  >
                    🔍 View on Explorer
                  </a>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
